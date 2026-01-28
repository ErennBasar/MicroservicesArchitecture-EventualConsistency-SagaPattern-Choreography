using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Order.API.DTOs;
using Order.API.Services.Abstractions;
using Shared.Messages;

namespace Order.API.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQueryRequest, OrderDto>
{
    private readonly IOrderService _orderService;
    private readonly IDistributedCache _distributedCache;
    

    public GetOrderByIdQueryHandler(IOrderService orderService, IDistributedCache distributedCache)
    {
        _orderService = orderService;
        _distributedCache = distributedCache;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQueryRequest request, CancellationToken cancellationToken)
    {
        string cacheKey = $"order_{request.OrderId}";
        OrderDto orderDto;

        // 1. REDIS KONTROLÜ (Manuel)
        var redisData = await _distributedCache.GetAsync(cacheKey, cancellationToken);

        if (redisData != null)
        {
            Console.WriteLine($"[REDIS] Veri Cache'den geldi: {request.OrderId} ⚡️");
            var serializedData = Encoding.UTF8.GetString(redisData);
            orderDto = JsonSerializer.Deserialize<OrderDto>(serializedData);
            return orderDto;
        }

        // 2. DB KONTROLÜ
        Console.WriteLine($"[DB] Veri Veritabanından çekiliyor: {request.OrderId} 🐢");

        var order = await _orderService.GetOrderByIdAsync(request.OrderId);
        if (order == null) return null;

        orderDto = new OrderDto
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            TotalPrice = order.TotalPrice,
            OrderDate = order.OrderDate,
            OrderStatus = order.OrderStatus,
            OrderItems = order.OrderItems.Select(oi => new CreateOrderItemDto 
            {
                ProductId = oi.ProductId,
                Count = oi.Count,
                Price = oi.Price
            }).ToList()
        };

        // 3. REDIS'E YAZMA (Manuel)
        var serializedOrder = JsonSerializer.Serialize(orderDto);
        var dataToCache = Encoding.UTF8.GetBytes(serializedOrder);

        var cacheOptions = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));

        await _distributedCache.SetAsync(cacheKey, dataToCache, cacheOptions, cancellationToken);

        return orderDto;
    }
}