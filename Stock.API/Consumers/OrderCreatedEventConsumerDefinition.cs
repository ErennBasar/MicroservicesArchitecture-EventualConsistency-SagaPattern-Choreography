using MassTransit;
using Shared;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumerDefinition : ConsumerDefinition<OrderCreatedEventConsumer>
{
    public OrderCreatedEventConsumerDefinition()
    {
        // Kuyruk ismini burada belirtiyoruz
        EndpointName = RabbitMqSettings.StockOrderCreatedEventQueue;
        
        // Bu consumer için aynı anda kaç mesajın işleneceğini belirler (Opsiyonel)
        //ConcurrentMessageLimit = 4; 
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator, 
        IConsumerConfigurator<OrderCreatedEventConsumer> consumerConfigurator, 
        IRegistrationContext context)
    {
        // MongoDB Outbox için transaction filtresi ekle
        endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 500, 1000, 2000));
        
        // Outbox middleware'ini bu consumer'a zorla enjekte ediyoruz.
        endpointConfigurator.UseMongoDbOutbox(context);
        
        // *** MongoDB Session/Transaction kullanımı için ***
        //endpointConfigurator.UseInMemoryOutbox(context);
    }
}