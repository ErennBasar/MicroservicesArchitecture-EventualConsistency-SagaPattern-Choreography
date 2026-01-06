namespace Order.API.DTOs;

public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public int Count { get; set; } 
    public decimal Price { get; set; }
}