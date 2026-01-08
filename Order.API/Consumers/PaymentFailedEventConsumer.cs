using MassTransit;
using Order.API.Models;
using Order.API.Models.Enums;
using Shared.Events;

namespace Order.API.Consumers;

public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly OrderApiDbContext _orderApiDbContext;

    public PaymentFailedEventConsumer(OrderApiDbContext orderApiDbContext)
    {
        _orderApiDbContext = orderApiDbContext;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var order = await _orderApiDbContext.Orders.FindAsync(context.Message.OrderId);
        if(order == null)
            throw new InvalidOperationException($"Order not found for OrderId: {context.Message.OrderId}");

        order.OrderStatus = OrderStatus.Failed;
        await _orderApiDbContext.SaveChangesAsync();
    }
}