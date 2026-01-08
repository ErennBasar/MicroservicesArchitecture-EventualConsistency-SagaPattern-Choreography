using MassTransit;
using Payment.API.Consumers;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<StockReservedEventConsumer>();

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

