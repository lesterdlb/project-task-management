using ProjectManagement.Api;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddApiServices()
    .AddDatabase();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    await app.ApplyMigrationsAsync();

    await app.SeedInitialDataAsync();
}
else
{
    app.UseExceptionHandler();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStatusCodePages();

app.MapSliceEndpoints();

app.MapGet("/", () => "Welcome to ProjectManagement.Api");

await app.RunAsync();
