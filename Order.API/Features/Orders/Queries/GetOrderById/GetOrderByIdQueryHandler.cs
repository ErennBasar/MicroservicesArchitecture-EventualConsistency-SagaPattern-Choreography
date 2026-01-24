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

        // 2. REDIS KONTROLÜ (Önce RAM'e bak)
        var redisData = await _distributedCache.GetAsync(cacheKey, cancellationToken);

        if (redisData != null)
        {
            Console.WriteLine($"[REDIS] Veri Cache'den geldi: {request.OrderId} ⚡️");
            
            var serializedData = Encoding.UTF8.GetString(redisData);
            orderDto = JsonSerializer.Deserialize<OrderDto>(serializedData);
            
            return orderDto;
        }
        
        // 3. VERİTABANI KONTROLÜ (Cache'de yoksa DB'ye git) 🐢
        Console.WriteLine($"[DB] Veri Veritabanından çekiliyor: {request.OrderId} 🐢");

        // Service üzerinden veriyi çekiyoruz (Senin yapında EventStore'dan veya ReadDb'den gelebilir)
        var order = await _orderService.GetOrderByIdAsync(request.OrderId);

        if (order == null)
            return null; 

        // Entity -> DTO Çevrimi (Manuel Mapping)
        orderDto = new OrderDto
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            TotalPrice = order.TotalPrice,
            OrderDate = order.OrderDate,
            OrderStatus = order.OrderStatus,
            OrderItems = order.OrderItems.Select(oi => new CreateOrderItemDto()
            {
                ProductId = oi.ProductId, // Entity'deki property isimleri
                Count = oi.Count,
                Price = oi.Price
            }).ToList()
        };
        
        // 4. REDIS'E YAZMA (Bir sonraki istek için sakla) 
        var serializedOrder = JsonSerializer.Serialize(orderDto);
        var dataToCache = Encoding.UTF8.GetBytes(serializedOrder);

        var cacheOptions = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(DateTime.Now.AddMinutes(10)) // 10 dakika dursun
            .SetSlidingExpiration(TimeSpan.FromMinutes(2));     // 2 dakika kimse sormazsa silinsin

        await _distributedCache.SetAsync(cacheKey, dataToCache, cacheOptions, cancellationToken);

        return orderDto;
    }
}