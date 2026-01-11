using MassTransit;
using MediatR;
using Order.API.Models;
using Order.API.Models.Entities;
using Order.API.Models.Enums;
using Shared.Events;
using Shared.Messages;

namespace Order.API.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommandRequest, CreateOrderCommandResponse>
{
    private readonly OrderApiDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOrderCommandHandler(OrderApiDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    
    public async Task<CreateOrderCommandResponse> Handle(CreateOrderCommandRequest request, CancellationToken cancellationToken)
    {
        var newOrder = new Models.Entities.Order()
        {
            OrderId = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            OrderDate = DateTime.UtcNow,
            OrderStatus = OrderStatus.Suspend,
            OrderItems = new List<OrderItem>()
        };

        foreach (var item in request.OrderItems)
        {
            newOrder.OrderItems.Add(new OrderItem
            {
                Count = item.Count,
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Price = item.Price
            });
        }

        newOrder.TotalPrice = newOrder.OrderItems.Sum(s => s.Count * s.Price);

        await _dbContext.Orders.AddAsync(newOrder);

        var orderCreatedEvent = new OrderCreatedEvent()
        {
            CustomerId = newOrder.CustomerId,
            OrderId = newOrder.OrderId,
            TotalPrice = newOrder.TotalPrice,
            OrderItems = newOrder.OrderItems.Select(s => new OrderItemMessage()
            {
                ProductId = s.ProductId,
                Count = s.Count,
            }).ToList()
        };

        await _publishEndpoint.Publish(orderCreatedEvent);

        await _dbContext.SaveChangesAsync();

        return new CreateOrderCommandResponse()
        {
            IsSucces = true,
            OrderId = newOrder.OrderId,
            Message = "Sipariş başarıyla alındı ve işleme kondu"
        };
    }
}