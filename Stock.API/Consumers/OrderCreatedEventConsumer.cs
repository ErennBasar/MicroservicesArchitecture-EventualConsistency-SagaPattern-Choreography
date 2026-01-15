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
    private readonly MongoDbService _mongoDbService;

    public OrderCreatedEventConsumer(
        IMongoCollection<Models.Stock> stockCollection, 
        ISendEndpointProvider sendEndpointProvider, 
        IPublishEndpoint publishEndpoint, 
        MongoDbService mongoDbService)
    {
        _stockCollection = stockCollection;
        _sendEndpointProvider = sendEndpointProvider;
        _publishEndpoint = publishEndpoint;
        _mongoDbService = mongoDbService;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var session = context.GetPayload<IClientSessionHandle>();

        List<bool> stockResult = new();

        // Stok kontrolü
        foreach (OrderItemMessage orderItem in context.Message.OrderItems)
        {
            var hasStock = await (await _stockCollection.FindAsync(
                    session,
                    s => s.ProductId == orderItem.ProductId && s.Count >= orderItem.Count))
                .AnyAsync();

            stockResult.Add(hasStock);
        }

        if (stockResult.TrueForAll(t => t.Equals(true)))
        {
            // Stok düş
            foreach (OrderItemMessage orderItem in context.Message.OrderItems)
            {
                Models.Stock stock = await (await _stockCollection.FindAsync(
                        session,
                        f => f.ProductId == orderItem.ProductId))
                    .FirstOrDefaultAsync();

                stock.Count -= orderItem.Count;

                var filter = Builders<Models.Stock>.Filter.Eq(t => t.ProductId, orderItem.ProductId);
                await _stockCollection.FindOneAndReplaceAsync(session, filter, stock);
            }

            // Stok rezerve edildi eventi
            StockReservedEvent stockReservedEvent = new()
            {
                CustomerId = context.Message.CustomerId,
                OrderId = context.Message.OrderId,
                TotalPrice = context.Message.TotalPrice,
                OrderItems = context.Message.OrderItems
            };

            await _publishEndpoint.Publish(stockReservedEvent);
        }
        else
        {
            // Stok yetersiz eventi
            StockNotReservedEvent stockNotReservedEvent = new()
            {
                CustomerId = context.Message.CustomerId,
                OrderId = context.Message.OrderId,
                Message = "Insufficient stock",
            };

            await _publishEndpoint.Publish(stockNotReservedEvent);
            await Console.Out.WriteLineAsync($"Stock not reserved for Order Id: {context.Message.OrderId}");
        }
    }
}