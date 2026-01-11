using EventStore.Client;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.DTOs;
using Order.API.Models;
using Order.API.Models.Enums;
using Order.API.Services;
using Order.API.Services.Abstractions;
using Order.API.Services.Concretes;
using Shared;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();  

builder.Services.AddDbContext<OrderApiDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrderDbPgSQL"));
});

builder.Services.AddSingleton(s =>
{
    var settings = EventStoreClientSettings.Create("esdb://localhost:2113?tls=false");
    return new EventStoreClient(settings);
});

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<PaymentCompletedEventConsumer>();
    cfg.AddConsumer<PaymentFailedEventConsumer>();
    cfg.AddConsumer<StockNotReservedEventConsumer>();
    
    cfg.AddEntityFrameworkOutbox<OrderApiDbContext>(outbox =>
    {
        outbox.QueryDelay = TimeSpan.FromSeconds(5); // 5 saniyede bir kuyruğu kontrol et
        outbox.UsePostgres(); 
        outbox.UseBusOutbox(); // "Namus" ayarı: Event'leri direkt atma, önce Outbox'a koy.
    });
    
    cfg.UsingRabbitMq((context, configurator) =>
    {
        var rabbitMqUri = builder.Configuration.GetConnectionString("RabbitMq");
        
        if(rabbitMqUri != null)
            configurator.Host(new Uri(rabbitMqUri)); 
        
        configurator.ReceiveEndpoint(RabbitMqSettings.OrderPaymentCompletedEventQueue,e =>
            e.ConfigureConsumer<PaymentCompletedEventConsumer>(context));
        
        configurator.ReceiveEndpoint(RabbitMqSettings.OrderPaymentFailedEventQueue, e => 
            e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
        
        configurator.ReceiveEndpoint(RabbitMqSettings.OrderStockNotReservedEventQueue, e => 
            e.ConfigureConsumer<StockNotReservedEventConsumer>(context));
    });
});

builder.Services.AddSingleton<EventStoreService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
