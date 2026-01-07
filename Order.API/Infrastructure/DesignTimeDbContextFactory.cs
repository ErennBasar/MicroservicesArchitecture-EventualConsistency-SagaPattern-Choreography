using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Order.API.Models;

namespace Order.API.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrderApiDbContext>
{
    public OrderApiDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("OrderDbPgSQL");
        
        var optionsBuilder = new DbContextOptionsBuilder<OrderApiDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new OrderApiDbContext(optionsBuilder.Options);
    }
}