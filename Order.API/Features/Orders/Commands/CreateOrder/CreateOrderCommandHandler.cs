using MassTransit;
using MediatR;
using Order.API.Models;
using Order.API.Models.Entities;
using Order.API.Models.Enums;
using Order.API.Services;
using Shared.Events;
using Shared.Messages;

namespace Order.API.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommandRequest, CreateOrderCommandResponse>
{
    private readonly EventStoreService _eventStoreService;
    
    // private readonly OrderApiDbContext _dbContext;
    // private readonly IPublishEndpoint _publishEndpoint;

    public CreateOrderCommandHandler(OrderApiDbContext dbContext, IPublishEndpoint publishEndpoint, EventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
        
        // _dbContext = dbContext;
        // _publishEndpoint = publishEndpoint;
    }
    
    public async Task<CreateOrderCommandResponse> Handle(CreateOrderCommandRequest request, CancellationToken cancellationToken)
    {
        // 1. Önce gerekli ID'leri oluşturalım
        var orderId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        // 2. Olayı (Event) Hazırla
        // Bu olay artık bizim veritabanı satırımız gibi davranacak.
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = orderId,
            CustomerId = request.CustomerId,
            TotalPrice = request.OrderItems.Sum(x => x.Count * x.Price), // Fiyatı hesapla
            OrderItems = request.OrderItems.Select(oi => new OrderItemMessage
            {
                ProductId = oi.ProductId,
                Count = oi.Count,
                Price = oi.Price
            }).ToList()
        };
        
        var eventMetadata = new
        {
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            UserId = request.CustomerId // Opsiyonel: İşlemi yapan kim?
        };

        // 3. Event Store'a Gönder! 🚀
        // Stream Adı Önemli: Her siparişin kendi akışı (stream) olur.
        // Örn: "Order-550e8400-e29b-..."
        var streamName = $"Order-{orderId}";

        await _eventStoreService.AppendToStreamAsync(
            streamName: streamName,
            eventDataList: new[] { orderCreatedEvent },
            metadata: eventMetadata
        );

        // 4. Cevap Dön
        return new CreateOrderCommandResponse
        {
            IsSuccess = true,
            OrderId = orderId,
            Message = "Sipariş Event Store'a başarıyla işlendi!"
        };
    }
}