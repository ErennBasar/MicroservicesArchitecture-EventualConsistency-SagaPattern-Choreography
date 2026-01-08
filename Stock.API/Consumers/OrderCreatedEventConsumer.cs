using MassTransit;
using MongoDB.Driver;
using Shared.Events;
using Shared.Messages;
using Stock.API.Services;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IMongoCollection<Models.Stock> _stockCollection;

    public OrderCreatedEventConsumer(IMongoCollection<Models.Stock> stockCollection)
    {
        _stockCollection = stockCollection;
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
                await _stockCollection.FindOneAndReplaceAsync(t => t.ProductId == orderItem.ProductId, stock);
            }
        }
        else
        {
            //stok işlemi başarısız
        }
    }
}