using MediatR;
using Order.API.DTOs;

namespace Order.API.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryRequest : IRequest<OrderDto>
{
    public Guid OrderId { get; set; }
    
    public GetOrderByIdQueryRequest(Guid orderId)
    {
        OrderId = orderId;
    }
}