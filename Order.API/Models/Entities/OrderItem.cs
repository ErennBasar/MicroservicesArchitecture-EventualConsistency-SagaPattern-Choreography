using System.ComponentModel.DataAnnotations.Schema;

namespace Order.API.Models.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Count { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public Order Order { get; set; } = null!;

}