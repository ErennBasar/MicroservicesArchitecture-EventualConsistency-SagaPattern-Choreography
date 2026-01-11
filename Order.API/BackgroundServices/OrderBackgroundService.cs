using System.Text;
using System.Text.Json;
using MassTransit;
using Order.API.Models;
using Order.API.Models.Entities;
using Order.API.Models.Enums;
using Order.API.Services;
using Shared.Events;

namespace Order.API.BackgroundServices;

public class OrderBackgroundService : BackgroundService
{
    private readonly EventStoreService _eventStoreService;
    private readonly IServiceProvider _serviceProvider; // DbContext ve IPublishEndpoint için Scope lazım

    public OrderBackgroundService(EventStoreService eventStoreService, IServiceProvider serviceProvider)
    {
        _eventStoreService = eventStoreService;
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // "$ce-Order" -> Order ile başlayan tüm streamleri (Order-1, Order-2...) buraya toplar.
        // Bu kanala abone oluyoruz.
        await _eventStoreService.SubscribeToStreamAsync(
            streamName: "$ce-Order", 
            eventAppeared: async (subscription, resolvedEvent, token) => 
            {
                // Gelen olayın tipi ne? (OrderCreatedEvent, OrderStatusChangedEvent vs.)
                var eventType = resolvedEvent.Event.EventType;
                var streamId = resolvedEvent.Event.EventStreamId;

                // CASUS 1: Buraya geliyorsa bağlantı var demektir.
                Console.WriteLine($"[GELEN EVENT] Stream: {streamId}, Tip: {eventType}");
                
                // Olayın verisini JSON stringine çevir
                var eventDataJson = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);

                // --- KRİTİK NOKTA: DbContext Scope Yönetimi ---
                // BackgroundService Singleton'dır (Uygulama boyunca 1 tane).
                // Ama DbContext Scoped'dur (Her işlemde yenilenir).
                // O yüzden manuel olarak Scope açıyoruz.
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<OrderApiDbContext>();
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                switch (eventType)
                {
                    case nameof(OrderCreatedEvent):
                        
                        Console.WriteLine($"✅ Event Tipi Eşleşti! ({eventType}) İşleniyor...");
                        // 1. Eventi Deserialize et (Nesneye çevir)
                        var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(eventDataJson);

                        if (orderEvent != null)
                        {
                            // 2. PostgreSQL'deki "Orders" tablosuna (Read Model) YAZ
                            // Önce var mı diye bak (Idempotency - Çift kayıt olmasın)
                            var existOrder = await dbContext.Orders.FindAsync(orderEvent.OrderId);
                            if (existOrder == null)
                            {
                                var newOrder = new Models.Entities.Order
                                {
                                    OrderId = orderEvent.OrderId,
                                    CustomerId = orderEvent.CustomerId,
                                    TotalPrice = orderEvent.TotalPrice,
                                    OrderDate = DateTime.UtcNow,
                                    OrderStatus = OrderStatus.Suspend,
                                    OrderItems = orderEvent.OrderItems.Select(oi => new OrderItem
                                    {
                                        Id = Guid.NewGuid(),
                                        ProductId = oi.ProductId,
                                        Count = oi.Count,
                                        Price = oi.Price
                                    }).ToList()
                                };

                                await dbContext.Orders.AddAsync(newOrder);
                                
                                Console.WriteLine($"✅ PostgreSQL Güncellendi: Sipariş {newOrder.OrderId}");

                                // 3. RabbitMQ'ya Mesajı FIRLAT (Stock.API duysun diye)
                                // NOT: Buradaki event, Outbox ile değil direkt gidiyor. 
                                // Çünkü burası zaten Event Store'dan besleniyor, burası çökerse Event Store kaldığı yerden devam eder.
                                await publishEndpoint.Publish(orderEvent);
                                
                                await dbContext.SaveChangesAsync();
                                Console.WriteLine($"📨 RabbitMQ'ya Gönderildi: Sipariş {newOrder.OrderId}");
                            }
                            Console.WriteLine($"💾 VERİTABANINA YAZILDI: Sipariş {orderEvent.OrderId}");
                        }
                        break;
                    
                    default:
                        Console.WriteLine($"❌ Event Tipi Eşleşmedi veya Tanımsız. Gelen: {eventType}");
                        break;

                    // İleride başka eventler gelirse buraya case ekleyeceğiz
                    // case "OrderCancelledEvent": ...
                }
            }
        );
    }
}