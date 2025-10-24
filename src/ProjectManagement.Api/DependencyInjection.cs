using System.Dynamic;
using System.Globalization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.Models;
using ProjectManagement.Api.Common.Persistence;
using ProjectManagement.Api.Common.Services;
using ProjectManagement.Api.Common.Services.Sorting;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Features.Users;
using ProjectManagement.Api.Mediator;
using ProjectManagement.Api.Mediator.Behaviors;
using ProjectManagement.Api.Middlewares;
using Serilog;

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
        builder.Services
            .AddScoped<IQueryHandler<GetUsers.GetUsersQuery, PaginationResult<ExpandoObject>>,
                GetUsers.GetUsersQueryHandler>();

        builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        builder.Services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly,
            includeInternalTypes: true);

        builder.Services.AddTransient<SortMappingProvider>();
        builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<GetUsers.UserDto, User>>(_ =>
            GetUsers.UserMappings.SortMapping);

        builder.Services.AddTransient<DataShapingService>();

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

    public static WebApplicationBuilder AddErrorHandling(this WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        return builder;
    }
}
