using LibraryApi.Api.Middleware;
using LibraryApi.Application;
using LibraryApi.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var cfg = builder.Configuration;

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddApplication();
builder.Services.AddPersistence(cfg);

builder.Services.AddControllers(options =>
{
    // Let FluentValidation in the mediator pipeline own validation (422), instead of MVC
    // rejecting missing or empty strings with its own 400 before the handler is reached
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

await app.Services.InitializeDatabaseAsync(seedDemoData: app.Environment.IsDevelopment());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(endpointPrefix: "/");
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();

// Make Program accessible to integration tests
public partial class Program { }
