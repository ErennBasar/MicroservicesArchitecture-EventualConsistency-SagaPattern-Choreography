using Order.API.DTOs;

namespace Order.API.Services.Abstractions;

public interface IOrderService
{
    Task CreateOrder(CreateOrderDto createOrderDto);
}