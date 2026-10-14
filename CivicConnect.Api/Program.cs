using CivicConnect.Infrastructure.Data;
using System.Text.Json.Serialization;
using CivicConnect.Core.Interfaces;
using CivicConnect.Core.Observers;
using CivicConnect.Core.Services;
using CivicConnect.Core.Strategies;
using CivicConnect.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing from appsettings.json.");
    
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddSingleton<IPriorityCalculationStrategy, EmergencyPriorityStrategy>();
builder.Services.AddSingleton<IPriorityCalculationStrategy, StandardPriorityStrategy>();
builder.Services.AddSingleton<PriorityCalculationContext>();
builder.Services.AddScoped<ITicketObserver, AuditLogTicketObserver>();
builder.Services.AddScoped<ITicketService, TicketService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using (var scope = app.Services.CreateScope())
    {
    var dbContext = scope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
    dbContext.Database.EnsureCreated();
    }
}

app.MapControllers();
app.Run();

public partial class Program { }