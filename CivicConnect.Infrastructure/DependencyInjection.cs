using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CivicConnect.Core.Interfaces;
using CivicConnect.Infrastructure.Data;
using CivicConnect.Infrastructure.Repositories;

namespace CivicConnect.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // 1. Register DbContext (SQLite)
        services.AddDbContext<CivicConnectDbContext>(options =>
            options.UseSqlite(connectionString));

        // 2. Register Generic Repository (for basic CRUD operations across all entities)
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // 3. Register Specific Repositories & Unit of Work
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}