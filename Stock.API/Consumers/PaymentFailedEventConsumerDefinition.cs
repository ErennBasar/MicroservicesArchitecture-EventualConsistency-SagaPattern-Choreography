using MassTransit;
using Shared;

namespace Stock.API.Consumers;

public class PaymentFailedEventConsumerDefinition  : ConsumerDefinition<PaymentFailedEventConsumer>
{
    public PaymentFailedEventConsumerDefinition()
    {
        EndpointName = RabbitMqSettings.StockPaymentFailedEventQueue;
    }
    
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<PaymentFailedEventConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 500, 1000, 2000));
        endpointConfigurator.UseMongoDbOutbox(context);
    }
}