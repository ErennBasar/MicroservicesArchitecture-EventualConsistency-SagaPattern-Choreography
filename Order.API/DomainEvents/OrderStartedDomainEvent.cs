using System.Runtime.InteropServices;
using MediatR;

namespace Order.API.DomainEvents;

public class OrderStartedDomainEvent : INotification
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
}