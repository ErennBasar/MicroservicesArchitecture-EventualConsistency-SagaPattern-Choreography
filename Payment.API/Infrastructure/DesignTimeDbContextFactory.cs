using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Payment.API.Models;

namespace Payment.API.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("PaymentDbPgSQL");
        var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new PaymentDbContext(optionsBuilder.Options);
    }
}