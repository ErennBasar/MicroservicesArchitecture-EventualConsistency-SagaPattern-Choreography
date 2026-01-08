using MassTransit;
using MongoDB.Driver;
using Shared.Events;

namespace Stock.API.Consumers;

public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly IMongoCollection<Models.Stock> _stockCollection;

    public PaymentFailedEventConsumer(IMongoCollection<Models.Stock> stockCollection)
    {
        _stockCollection = stockCollection;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        if (context.Message.OrderItems == null || !context.Message.OrderItems.Any())
        {
            Console.WriteLine($"HATA: İade edilecek ürün listesi boş geldi! OrderId: {context.Message.OrderId}");
            return;
        }
            
        foreach (var item in context.Message.OrderItems)
        {
            var filter = Builders<Models.Stock>.Filter.Eq(s => s.ProductId, item.ProductId);
            
            var update = Builders<Models.Stock>.Update.Inc(s => s.Count, item.Count);
            
            var result = await _stockCollection.UpdateOneAsync(filter, update);

            // Eğer en az 1 kayıt etkilendiyse (ModifiedCount > 0) işlem başarılıdır
            if (result.ModifiedCount > 0)
            {
                Console.WriteLine($"Stok iade edildi! Ürün: {item.ProductId} - İade Adedi: {item.Count}");
            }
            else
            {
                // Ürün veritabanında bulunamamış demektir
                Console.WriteLine($"UYARI: Ürün bulunamadığı için stok iade edilemedi! ProductId: {item.ProductId}");
            } 
        }
    }
}