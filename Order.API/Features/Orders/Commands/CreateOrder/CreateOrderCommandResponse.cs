using System.Runtime.InteropServices;

namespace Order.API.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandResponse
{
    public bool IsSuccess { get; set; }
    public Guid OrderId { get; set; }
    public string? Message { get; set; }
}