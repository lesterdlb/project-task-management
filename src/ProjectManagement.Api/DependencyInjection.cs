using System.Globalization;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.DTOs.User;
using ProjectManagement.Api.Common.Mappings;
using ProjectManagement.Api.Common.Persistence;
using ProjectManagement.Api.Common.Services;
using ProjectManagement.Api.Common.Services.Auth;
using ProjectManagement.Api.Common.Services.Email;
using ProjectManagement.Api.Common.Services.Sorting;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Mediator;
using ProjectManagement.Api.Mediator.Behaviors;
using ProjectManagement.Api.Middlewares;
using Serilog;
using TokenOptions = ProjectManagement.Api.Common.Services.Auth.TokenOptions;

namespace ProjectManagement.Api;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Host.UseSerilog((_, _, configuration) =>
        {
            configuration
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture);
        }, writeToProviders: true);

        builder.Services.AddOpenApi();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSlices();

        builder.Services.AddScoped<IMediator, Mediator.Mediator>();
        RegisterUserQueryAndCommandHandlers(builder);

        builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        builder.Services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly,
            includeInternalTypes: true);

        builder.Services.AddTransient<ISortMappingProvider, SortMappingProvider>();
        builder.Services.AddSingleton<ISortMappingDefinition,
            SortMappingDefinition<UserDto, User>>(_ => UserMappings.UserSortMapping);

        builder.Services.AddTransient<IDataShapingService, DataShapingService>();

        builder.Services.AddScoped<IEmailService, EmailService>();

        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ProjectManagementDbContext>(options =>
            options
                .UseNpgsql(
                    builder.Configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, "public"))
                .UseSnakeCaseNamingConvention());

        return builder;
    }

    public static WebApplicationBuilder AddAuthenticationServices(this WebApplicationBuilder builder)
    {
        var tokenSection = builder.Configuration.GetSection(nameof(TokenOptions));
        builder.Services.Configure<TokenOptions>(tokenSection);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<ITokenService, TokenService>();

        builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                options.SignIn.RequireConfirmedEmail = true;

                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ProjectManagementDbContext>()
            .AddDefaultTokenProviders();

        var jwtOptions = tokenSection.Get<TokenOptions>() ??
                         throw new InvalidOperationException("JWT configuration is missing");

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                };
            });

        builder.Services.AddAuthorization();

        return builder;
    }

    public static WebApplicationBuilder AddErrorHandling(this WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<DatabaseExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        return builder;
    }

    private static void RegisterUserQueryAndCommandHandlers(WebApplicationBuilder builder)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register query handlers
        var queryHandlerTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        t.GetInterfaces().Any(i => i.IsGenericType &&
                                                   i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)));

        foreach (var queryHandler in queryHandlerTypes)
        {
            var interfaceType = queryHandler.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));
            builder.Services.AddScoped(interfaceType, queryHandler);
        }

        // Register command handlers
        var commandHandlerTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        t.GetInterfaces().Any(i => i.IsGenericType &&
                                                   i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)));

        foreach (var commandHandler in commandHandlerTypes)
        {
            var interfaceType = commandHandler.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));
            builder.Services.AddScoped(interfaceType, commandHandler);
        }
    }
}
