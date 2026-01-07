using MassTransit;
using Stock.API.Consumers;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<OrderCreatedEventConsumer>();
    
     cfg.UsingRabbitMq((context, configurator) =>
    {
        var rabbitMqUri = builder.Configuration.GetConnectionString("RabbitMq");
        
        if(rabbitMqUri != null)
            configurator.Host(new Uri(rabbitMqUri));
        
        configurator.ReceiveEndpoint("stock-order-created-event-queue", e => 
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
    }) ;
});

builder.Services.AddSingleton<MongoDbService>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.Run();
