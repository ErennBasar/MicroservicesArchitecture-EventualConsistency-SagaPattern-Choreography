using System.ComponentModel.DataAnnotations.Schema;
using Order.API.Models.Enums;

namespace Order.API.Models.Entities;

public class Order
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; } 
    [Column(TypeName = "decimal(18.2)")]
    public Decimal TotalPrice { get; set; }
    public string? FailMessage { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
}