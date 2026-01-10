using MassTransit;
using Order.API.DTOs;
using Order.API.Models;
using Order.API.Models.Entities;
using Order.API.Models.Enums;
using Order.API.Services.Abstractions;
using Shared.Events;
using Shared.Messages;

namespace Order.API.Services.Concretes;

public class OrderService : IOrderService
{
    private readonly OrderApiDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndPoint;

    public OrderService(OrderApiDbContext dbContext, IPublishEndpoint publishEndPoint)
    {
        _dbContext = dbContext;
        _publishEndPoint = publishEndPoint;
    }

    public async Task CreateOrder(CreateOrderDto createOrderDto)
    {
        var newOrder = new Models.Entities.Order
        { 
            CustomerId = createOrderDto.CustomerId,
            OrderId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            OrderStatus = OrderStatus.Suspend
        };

        foreach (var item in createOrderDto.OrderItems)
        {
            newOrder.OrderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Count = item.Count,
                Price = item.Price
            });
        }

        newOrder.TotalPrice = createOrderDto.OrderItems.Sum(oi => oi.Count * oi.Price);
        
        // 2. Siparişi Context'e Ekle (Ama henüz kaydetme)
        await _dbContext.Orders.AddAsync(newOrder);

        OrderCreatedEvent orderCreatedEvent = new()
        {
            OrderId = newOrder.OrderId,
            CustomerId = newOrder.CustomerId,
            TotalPrice = newOrder.TotalPrice,
            OrderItems = newOrder.OrderItems.Select(i => new OrderItemMessage
            {
                ProductId = i.ProductId,
                Count = i.Count
            }).ToList()
        };
        
        // Publish işlemini SaveChanges'dan ÖNCE yapıyoruz.
        // MassTransit burada RabbitMQ'ya gitmez, Context'teki 'OutboxMessage' tablosuna bir kayıt ekler.
        await _publishEndPoint.Publish(orderCreatedEvent);
        //Tek seferde hem Siparişi hem de Mesajı veritabanına gömüyoruz.
        await _dbContext.SaveChangesAsync();
    }
}