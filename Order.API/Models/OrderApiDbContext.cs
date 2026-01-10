using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // InboxState:Gelen messajların takibi (Order.API ileride mesaj alırsa diye)
        modelBuilder.AddInboxStateEntity();
        // OutboxMessage: Gönderilecek mesajlar buraya yazılacak
        modelBuilder.AddOutboxMessageEntity();
        // OutboxState: Outbox'ın durumunu tutar
        modelBuilder.AddOutboxStateEntity();
    }
}