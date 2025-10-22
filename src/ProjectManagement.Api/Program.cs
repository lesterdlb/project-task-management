using ProjectManagement.Api;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddApiServices()
    .AddDatabase();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();
app.UseStatusCodePages();

app.MapGet("/", () => "Welcome to ProjectManagement.Api");

await app.RunAsync();