using CivicConnect.Core.Exceptions;
using CivicConnect.Core.Interfaces;
using CivicConnect.Core.Observers;
using CivicConnect.Core.Services;
using CivicConnect.Core.Strategies;
using CivicConnect.Infrastructure;
using CivicConnect.Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- Web API basics ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        // Show enums as "Critical" instead of 3 in requests and responses
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Infrastructure (database, repositories, unit of work) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing from appsettings.json.");

builder.Services.AddInfrastructure(connectionString);

// --- Core wiring ---
builder.Services.AddSingleton<IPriorityCalculationStrategy, EmergencyPriorityStrategy>();
builder.Services.AddSingleton<IPriorityCalculationStrategy, StandardPriorityStrategy>();
builder.Services.AddSingleton<PriorityCalculationContext>();
builder.Services.AddScoped<ITicketObserver, AuditLogTicketObserver>();
builder.Services.AddScoped<ITicketService, TicketService>();

var app = builder.Build();

// --- Exception Handling Middleware ---
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        switch (exception)
        {
            case DomainRuleViolationException ruleEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = ruleEx.Message });
                break;

            case EntityNotFoundException notFoundEx:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new { error = notFoundEx.Message });
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
                break;
        }
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Properly create a scope to resolve DbContext and ensure DB creation
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        dbContext.Database.EnsureCreated();
    }
}

app.MapControllers();

app.Run();

// Lets a future integration-test project start the app in memory
public partial class Program { }