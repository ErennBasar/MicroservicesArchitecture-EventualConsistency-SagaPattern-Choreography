using Order.API.Models.Entities;

namespace Order.API.DTOs;

public class CreateOrderDto
{
    public Guid CustomerId { get; set; }
    public List<CreateOrderItemDto> OrderItems { get; set; }
}