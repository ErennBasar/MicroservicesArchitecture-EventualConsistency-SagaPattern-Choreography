using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Shared.Messages;
using Stock.API.Services;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IMongoCollection<Models.Stock> _stockCollection;
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderCreatedEventConsumer(IMongoCollection<Models.Stock> stockCollection, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
    {
        _stockCollection = stockCollection;
        _sendEndpointProvider = sendEndpointProvider;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        List<bool> stockResult = new(); //Siparişteki ürünlerin tümünün stock bilgisini tutmak için 
        
        foreach (OrderItemMessage orderItem in context.Message.OrderItems)
        {
            stockResult.Add(await (await _stockCollection.FindAsync(s => 
                s.ProductId == orderItem.ProductId && s.Count > orderItem.Count)).AnyAsync());
        }

        if (stockResult.TrueForAll(t => t.Equals(true)))
        {
            foreach (OrderItemMessage orderItem in context.Message.OrderItems)
            {
                Models.Stock stock = await (await _stockCollection.FindAsync(f => 
                    f.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();

                stock.Count -= orderItem.Count;
                await _stockCollection.FindOneAndReplaceAsync(t => 
                    t.ProductId == orderItem.ProductId, stock);
            }
            
            StockReservedEvent stockReservedEvent = new()
            {
                CustomerId = context.Message.CustomerId,
                OrderId = context.Message.OrderId,
                TotalPrice = context.Message.TotalPrice,
                OrderItems =  context.Message.OrderItems
            };
            await _publishEndpoint.Publish(stockReservedEvent);

            // ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri
            //     ($"queue:{ RabbitMqSettings.PaymentStockReservedEvent}"));
            // await sendEndpoint.Send(stockReservedEvent);
            await Console.Out.WriteLineAsync($"Stock reserved for Order Id: {context.Message.OrderId}");
        }
        else
        {
            StockNotReservedEvent stockNotReservedEvent = new()
            {
                CustomerId = context.Message.CustomerId,
                OrderId = context.Message.OrderId,
                Message = "Order Not Reserved",
            };
            await _publishEndpoint.Publish(stockNotReservedEvent);
            await Console.Out.WriteLineAsync($"Stock not reserved for Order Id: {context.Message.OrderId}");
        }
    }
}