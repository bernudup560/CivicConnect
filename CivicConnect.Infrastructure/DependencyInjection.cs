using CivicConnect.Infrastructure.Data;
using CivicConnect.Core.Interfaces;
using CivicConnect.Infrastructure.Repositories; // Uncomment if your UnitOfWork is in a Repositories folder
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CivicConnect.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection, string databaseConnectionString)
    {
        serviceCollection.AddDbContext<CivicConnectDbContext>(databaseOptions =>
            databaseOptions.UseSqlite(databaseConnectionString));

        serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>(); 
        
        return serviceCollection;
    }

    public static void EnsureDatabaseCreated(this IServiceProvider serviceProvider)
    {
        using var serviceScope = serviceProvider.CreateScope();
        var databaseContext = serviceScope.ServiceProvider.GetRequiredService<CivicConnectDbContext>();
        databaseContext.Database.EnsureCreated();
    }
}