using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Order.API.Models.Enums;
using Shared.Events;

namespace Order.API.Consumers;

public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
{
    private readonly OrderApiDbContext _orderApiDbContext;

    public PaymentCompletedEventConsumer(OrderApiDbContext orderApiDbContext)
    {
        _orderApiDbContext = orderApiDbContext;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        // 1
        var order = await _orderApiDbContext.Orders.FindAsync(context.Message.OrderId);
        if (order == null)
            throw new InvalidOperationException($"Order not found for OrderId: {context.Message.OrderId}");
        // FindAsync: Entity Framework'ün en akıllı metodudur. Önce hafızaya (RAM'deki Change Tracker'a) bakar, "Bu veri bende zaten var mı?" diye. Varsa veritabanına gitmez, direkt RAM'den getirir. Yoksa veritabanına gider. Primary Key (PK) sorgularında optimize edilmiştir.
        
        //2
        // Models.Entities.Order order = (await _orderApiDbContext.Orders.FirstOrDefaultAsync(f => 
        //     f.OrderId == context.Message.OrderId))!;
        // FirstOrDefaultAsync: Hafızaya bakmaz. Direkt SQL sorgusu (SELECT TOP 1...) oluşturur ve veritabanına gönderir.

        order.OrderStatus = OrderStatus.Completed;
        await _orderApiDbContext.SaveChangesAsync();
    }
}