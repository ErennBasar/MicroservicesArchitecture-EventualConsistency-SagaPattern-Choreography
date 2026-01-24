using Order.API.Models.Enums;

namespace Order.API.DTOs;

public class OrderDto
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus OrderStatus { get; set; } 
    public required List<CreateOrderItemDto> OrderItems { get; set; }
}