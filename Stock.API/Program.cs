using MassTransit;
using MongoDB.Driver;
using Shared;
using Stock.API.Consumers;
using Stock.API.Services;
using HealthChecks.UI.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NLog.Web;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

builder.Services.AddMassTransit(cfg =>
{
    //cfg.AddConsumer<OrderCreatedEventConsumer>();
    cfg.AddConsumer<PaymentFailedEventConsumer>(typeof(PaymentFailedEventConsumerDefinition));
    cfg.AddConsumer<OrderCreatedEventConsumer>(typeof(OrderCreatedEventConsumerDefinition));
    
    cfg.AddMongoDbOutbox(outbox =>
    {
        outbox.DisableInboxCleanupService(); // Test için cleanup'ı kapattık (Veriyi görelim diye)
        //outbox.UseBusOutbox(); // Inbox pattern'i aktif eder
        
        outbox.ClientFactory(provider => provider.GetRequiredService<MongoDbService>().Client);
        outbox.DatabaseFactory(provider => provider.GetRequiredService<MongoDbService>().Database);
        
        outbox.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
        outbox.QueryDelay = TimeSpan.FromSeconds(1);
        outbox.QueryMessageLimit = 100;
        
    });
    
     cfg.UsingRabbitMq((context, configurator) =>
    {
        var rabbitMqUri = builder.Configuration.GetConnectionString("RabbitMq");
        
        // Bağlantı adresi (amqp:// düzeltmesi için Uri kontrolü)
        if (!string.IsNullOrEmpty(rabbitMqUri))
        {
            // Eğer connection string "rabbitmq://" ile başlıyorsa düzelt
            if (rabbitMqUri.StartsWith("rabbitmq://"))
                rabbitMqUri = rabbitMqUri.Replace("rabbitmq://", "amqp://");
                
            configurator.Host(new Uri(rabbitMqUri));
        }
        
        // configurator.ReceiveEndpoint(RabbitMqSettings.StockOrderCreatedEventQueue, e => 
        //     e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        //
        // configurator.ReceiveEndpoint(RabbitMqSettings.StockPaymentFailedEventQueue, e => 
        //     e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
        
        configurator.ConfigureEndpoints(context);
    }) ;
});

builder.Services.AddHealthChecks()
    .AddMongoDb(
        sp => 
        {
            var connectionString = builder.Configuration.GetConnectionString("MongoDb");
            return new MongoClient(connectionString);
        },
        
        name: "MongoDB Check",
        failureStatus: HealthStatus.Unhealthy,
        tags: new string[] { "mongodb" }
    )
    .AddRabbitMQ(
        async sp => 
        {
            // 1. Connection String'i config'den al
            var connectionString = builder.Configuration.GetConnectionString("RabbitMq")!;

            // 2. TRICK BURADA: "rabbitmq://" ifadesini "amqp://" ile değiştiriyoruz.
            // Bu sayede appsettings.json dosyan MassTransit için uyumlu kalırken,
            // burada sürücüye (Driver) istediği formatı veriyoruz.
            var amqpUri = connectionString.Replace("rabbitmq://", "amqp://");

            var factory = new ConnectionFactory()
            {
                Uri = new Uri(amqpUri)
            };
        
            return await factory.CreateConnectionAsync();
        },
        name: "RabbitMQ Check",
        failureStatus: HealthStatus.Unhealthy,
        tags: new string[] { "rabbitmq" }
    );
var app = builder.Build();

app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var stockCollection = scope.ServiceProvider.GetRequiredService<IMongoCollection<Stock.API.Models.Stock>>();
    
    await MongoDbSeeder.Seed(stockCollection);
}

app.Run();
