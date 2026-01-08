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
        if (true)
        {
            //ödeme başarılı
        }
        else
        {
            //ödeme başarısız
        }
    }
}