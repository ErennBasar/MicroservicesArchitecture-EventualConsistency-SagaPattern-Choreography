using MediatR;
using Order.API.DTOs;

namespace Order.API.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandRequest : IRequest<CreateOrderCommandResponse>
{
    public Guid CustomerId { get; set; }
    public List<CreateOrderItemDto> OrderItems { get; set; }
}