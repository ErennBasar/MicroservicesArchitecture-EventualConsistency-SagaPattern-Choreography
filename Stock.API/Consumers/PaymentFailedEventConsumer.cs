using MassTransit;
using MongoDB.Driver;
using Shared.Events;
using Stock.API.Services;

namespace Stock.API.Consumers;

public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly IMongoCollection<Models.Stock> _stockCollection;
    private readonly ILogger<PaymentFailedEventConsumer> _logger;
    private readonly MongoDbService _mongoDbService;

    public PaymentFailedEventConsumer(IMongoCollection<Models.Stock> stockCollection, MongoDbService mongoDbService, ILogger<PaymentFailedEventConsumer> logger)
    {
        _stockCollection = stockCollection;
        _mongoDbService = mongoDbService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        _logger.LogInformation("PaymentFailedEvent received for OrderId: {OrderId}", context.Message.OrderId);

        using var session = await _mongoDbService.Client.StartSessionAsync();
        session.StartTransaction();
        
        if (context.Message.OrderItems == null || !context.Message.OrderItems.Any())
        {
            Console.WriteLine($"HATA: İade edilecek ürün listesi boş geldi! OrderId: {context.Message.OrderId}");
            return;
        }
            
        foreach (var item in context.Message.OrderItems)
        {
            // var filter = Builders<Models.Stock>.Filter.Eq(s => s.ProductId, item.ProductId);
            //
            // var update = Builders<Models.Stock>.Update.Inc(s => s.Count, item.Count);
            //
            // var result = await _stockCollection.UpdateOneAsync(filter, update);
            //
            // // Eğer en az 1 kayıt etkilendiyse (ModifiedCount > 0) işlem başarılıdır
            // if (result.ModifiedCount > 0)
            // {
            //     Console.WriteLine($"Stok iade edildi! Ürün: {item.ProductId} - İade Adedi: {item.Count}");
            // }
            // else
            // {
            //     // Ürün veritabanında bulunamamış demektir
            //     Console.WriteLine($"UYARI: Ürün bulunamadığı için stok iade edilemedi! ProductId: {item.ProductId}");
            // } 
            
            Models.Stock stock = await (await _stockCollection.FindAsync(
                    session,
                    s => s.ProductId == item.ProductId))
                .FirstOrDefaultAsync();

            stock.Count += item.Count;

            var filter = Builders<Models.Stock>.Filter.Eq(s => s.ProductId, item.ProductId);
            await _stockCollection.FindOneAndReplaceAsync(session, filter, stock);
        }

        await session.CommitTransactionAsync();
        
        _logger.LogInformation("Stock returned for Order Id: {OrderId}", context.Message.OrderId);
    }
}