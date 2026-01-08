using Shared.Events.Common;

namespace Shared.Events;

public class StockNotReservedEvent : IEvent
{
    public Guid CustomerId { get; set; }
    public Guid OrderId { get; set; }
    public string? Message { get; set; }
}