namespace Shared.Messages;

public class OrderItemMessage
{
    // Genelde d etay bilgiler tutulur
    public Guid ProductId { get; set; }
    public int Count { get; set; }
}