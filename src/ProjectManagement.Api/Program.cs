using ProjectManagement.Api;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddApiServices()
    .AddDatabase();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStatusCodePages();

app.MapGet("/", () => "Welcome to ProjectManagement.Api");

await app.RunAsync();
