using Microsoft.EntityFrameworkCore;
using Order.API.Models.Entities;

namespace Order.API.Models;

public class OrderApiDbContext : DbContext
{
    public OrderApiDbContext(DbContextOptions<OrderApiDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Entities.Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}