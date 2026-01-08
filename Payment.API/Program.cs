using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator => 
    configurator.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqUri = builder.Configuration.GetConnectionString("RabbitMq");
        
        if(rabbitMqUri != null)
            cfg.Host(new Uri(rabbitMqUri));
        
        
    }));

var app = builder.Build();

 
if (app.Environment.IsDevelopment())
{
    
}

app.Run();

