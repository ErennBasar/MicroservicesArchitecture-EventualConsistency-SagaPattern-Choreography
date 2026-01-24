using MediatR;

namespace Order.API.DomainEvents;

public class OrderStartedDomainEventHandler : INotificationHandler<OrderStartedDomainEvent>
{
    private readonly ILogger<OrderStartedDomainEventHandler> _logger;

    public OrderStartedDomainEventHandler(ILogger<OrderStartedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderStartedDomainEvent notification, CancellationToken cancellationToken)
    {
        // BURASI SADECE ORDER.API İÇİNDEKİ İŞLERİ YAPAR
        // Örn: Müşteriye "Siparişiniz alındı" maili atabilir (RabbitMQ'ya gerek duymadan).
        
        _logger.LogInformation($"[DOMAIN EVENT] Sipariş süreci başladı! " +
                               $"OrderId: {notification.OrderId}, " +
                               $"CustomerId: {notification.CustomerId}, " +
                               $"Price: {notification.TotalPrice}, " +
                               $"OrderDate: {notification.OrderDate}");
        
        // Simülasyon: Mail atıyor gibi yapalım
        // await _mailService.SendWelcomeMailAsync(notification.CustomerId);
        
        return;
    }
}