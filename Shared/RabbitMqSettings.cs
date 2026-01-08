namespace Shared;

public static class RabbitMqSettings
{
    public const string StockOrderCreatedEventQueue = "stock-order-created-event-queue";
    public const string PaymentStockReservedEventQueue = "payment-stock-reserved-event-queue";
    public const string OrderPaymentCompletedEventQueue = "order-payment-completed-event-queue";
    public const string OrderPaymentFailedEventQueue = "order-payment-failed-event-queue";
    public const string StockPaymentFailedEventQueue = "stock-payment-failed-event-queue";
}