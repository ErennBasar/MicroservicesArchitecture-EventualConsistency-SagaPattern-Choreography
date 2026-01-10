using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers;

public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public StockReservedEventConsumer(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<StockReservedEvent> context)
    {
        var limit = 3000;
        if (context.Message.TotalPrice <= limit)
        {
            PaymentCompletedEvent paymentCompletedEvent = new()
            {
                OrderId = context.Message.OrderId
            };
            await _publishEndpoint.Publish(paymentCompletedEvent);
            Console.WriteLine($"Ödeme Başarılı! Tutar: {context.Message.TotalPrice} TL - OrderId: {context.Message.OrderId}");
        }
        else
        {
            PaymentFailedEvent paymentFailedEvent = new()
            {
                OrderId = context.Message.OrderId,
                Message = "Bakiye yetersiz",
                OrderItems = context.Message.OrderItems
            };
            await _publishEndpoint.Publish(paymentFailedEvent);
            Console.WriteLine($"Ödeme Başarısız! Bakiye Yetersiz. Tutar: {context.Message.TotalPrice} TL");
        }
    }
}