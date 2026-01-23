using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Eureka;
using Ocelot.Provider.Polly;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Eureka;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAuthentication().AddJwtBearer("GatewayAuthScheme", options =>
{
    options.Authority = builder.Configuration["IdentityServerUrl"];
    options.Audience = "ResourceGateway";
    options.RequireHttpsMetadata = false;
});

// 1. Ocelot.json dosyasını konfigürasyona ekle
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddDiscoveryClient(builder.Configuration);

// 2. Ocelot Servislerini Ekle
builder.Services.AddOcelot()
    .AddEureka()
    .AddPolly();

var app = builder.Build();

// 3. Ocelot Middleware'ini Kullan
await app.UseOcelot();

app.Run();