using MassTransit;
using Microsoft.EntityFrameworkCore;
using NLog.Web;
using Payment.API.Consumers;
using Payment.API.Models;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLog();

builder.Services.AddDbContext<PaymentDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PaymentDbPgSQL"));
});

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<StockReservedEventConsumer>();
    
    configurator.AddEntityFrameworkOutbox<PaymentDbContext>(outbox =>
    {
        outbox.QueryDelay = TimeSpan.FromSeconds(5);
        outbox.UsePostgres();
        outbox.UseBusOutbox();
    });

    configurator.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqUri = builder.Configuration.GetConnectionString("RabbitMq");

        if (rabbitMqUri != null)
            cfg.Host(new Uri(rabbitMqUri));

        cfg.ReceiveEndpoint(RabbitMqSettings.PaymentStockReservedEventQueue, e =>
            e.ConfigureConsumer<StockReservedEventConsumer>(context));

    });
});


var app = builder.Build();

 
if (app.Environment.IsDevelopment())
{
    
}

app.Run();

