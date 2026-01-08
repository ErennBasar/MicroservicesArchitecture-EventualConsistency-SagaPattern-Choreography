using MassTransit;
using MongoDB.Driver;
using Shared;
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
        
        configurator.ReceiveEndpoint(RabbitMqSettings.StockOrderCreatedEventQueue, e => 
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
    }) ;
});

builder.Services.AddSingleton<MongoDbService>();

//  Consumer'ın veya Controller'ın kullanacağı Collection'ı servisten alıp dağıt
// (Scoped: Her istekte servisten koleksiyonu ister)
builder.Services.AddScoped<IMongoCollection<Stock.API.Models.Stock>>(sp =>
{
    // Konteynerdan bizim servisi çağır
    var mongoService = sp.GetRequiredService<MongoDbService>();
    
    // "Stocks" tablosunu getir
    return mongoService.GetCollection<Stock.API.Models.Stock>();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.Run();
