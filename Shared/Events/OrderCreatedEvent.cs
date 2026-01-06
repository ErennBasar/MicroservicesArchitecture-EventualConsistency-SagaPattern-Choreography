using Shared.Events.Common;
using Shared.Messages;

namespace Shared.Events;

public class OrderCreatedEvent : IEvent
{
    //Servisler arasında veriyi barındaracak türeden olması gerekiyor
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderItemMessage> OrderItems { get; set; }
}