using ProjectManagement.Api;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddApiServices()
    .AddDatabase()
    .AddAuthenticationServices()
    .AddErrorHandling();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    await app.ApplyMigrationsAsync();

    await app.SeedInitialDataAsync();
}

app.UseExceptionHandler();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

app.MapSliceEndpoints();

app.MapGet("/", () => "Welcome to ProjectManagement.Api");

await app.RunAsync();
