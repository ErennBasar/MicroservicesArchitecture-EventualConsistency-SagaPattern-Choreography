using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.DTOs;
using Order.API.Models;
using Order.API.Models.Enums;
using Shared;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSwaggerGen();  

builder.Services.AddDbContext<OrderApiDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrderDbPgSQL"));
});

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<PaymentCompletedEventConsumer>();
    cfg.AddConsumer<PaymentFailedEventConsumer>();
    cfg.AddConsumer<StockNotReservedEventConsumer>();
    
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();





app.Run();
