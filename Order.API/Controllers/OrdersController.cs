using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Features.Orders.Commands.CreateOrder;
using Order.API.Features.Orders.Queries.GetOrderById;
using Order.API.Services.Abstractions;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMediator _mediator;

        public OrdersController(IOrderService orderService, IMediator mediator)
        {
            _orderService = orderService;
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderCommandRequest request)
        {
            CreateOrderCommandResponse response = await _mediator.Send(request);

            if (response.IsSuccess)
                return Ok(response);
            
            return BadRequest(response.Message);
        }
        
        [HttpGet("{orderId}")] // URL Örneği: api/orders/b36cd...
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            // Mediator'a ID'yi paketleyip gönderiyoruz
            OrderDto response = await _mediator.Send(new GetOrderByIdQueryRequest(orderId));

            if (response == null)
                return NotFound("Böyle bir sipariş bulunamadı usta.");

            return Ok(response);
        }
    }
}
