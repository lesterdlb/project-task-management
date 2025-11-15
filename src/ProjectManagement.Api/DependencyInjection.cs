using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.DTOs.Project;
using ProjectManagement.Api.Common.DTOs.User;
using ProjectManagement.Api.Common.Mappings;
using ProjectManagement.Api.Common.Persistence;
using ProjectManagement.Api.Common.Services.Auth;
using ProjectManagement.Api.Common.Services.DataShaping;
using ProjectManagement.Api.Common.Services.Email;
using ProjectManagement.Api.Common.Services.Links;
using ProjectManagement.Api.Common.Services.Sorting;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Mediator;
using ProjectManagement.Api.Mediator.Behaviors;
using ProjectManagement.Api.Middlewares;
using Serilog;
using TokenOptions = ProjectManagement.Api.Common.Services.Auth.TokenOptions;

namespace ProjectManagement.Api;

public static class DependencyInjection
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddApiServices()
        {
            builder.Logging.ClearProviders();
            builder.Host.UseSerilog((_, _, configuration) =>
            {
                configuration
                    .MinimumLevel.Information()
                    .Enrich.FromLogContext()
                    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture);
            }, writeToProviders: true);

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            builder.Services.Configure<JsonOptions>(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            builder.Services.AddRouting(options => options.LowercaseUrls = true);

            builder.Services.Configure<MvcOptions>(options =>
            {
                var formatter = options.OutputFormatters
                    .OfType<SystemTextJsonOutputFormatter>()
                    .First();

                formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV1);
                formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV2);
                formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJson);
                formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV1);
                formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV2);
            });

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
            builder.Services.AddSingleton<ISortMappingDefinition,
                SortMappingDefinition<ProjectDto, Project>>(_ => ProjectMappings.ProjectSortMapping);

            builder.Services.AddTransient<IDataShapingService, DataShapingService>();

            builder.Services.AddScoped<IEmailService, EmailService>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<ILinkService, LinkService>();

            return builder;
        }

        public WebApplicationBuilder AddDatabase()
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

        public WebApplicationBuilder AddAuthenticationServices()
        {
            var tokenSection = builder.Configuration.GetSection(nameof(TokenOptions));
            builder.Services.Configure<TokenOptions>(tokenSection);

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

        public WebApplicationBuilder AddErrorHandling()
        {
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
            builder.Services.AddExceptionHandler<DatabaseExceptionHandler>();
            builder.Services.AddExceptionHandler<JsonExceptionHandler>();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            return builder;
        }

        public WebApplicationBuilder AddCors()
        {
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy
                        .WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            return builder;
        }

        private static void RegisterUserQueryAndCommandHandlers(WebApplicationBuilder applicationBuilder)
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
                applicationBuilder.Services.AddScoped(interfaceType, queryHandler);
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
                applicationBuilder.Services.AddScoped(interfaceType, commandHandler);
            }
        }
    }
}
