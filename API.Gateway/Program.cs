using System.Text;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Eureka;
using Ocelot.Provider.Polly;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Eureka;

var builder = WebApplication.CreateBuilder(args);

var secretKey = builder.Configuration["JwtSettings:SecretKey"];
    
builder.Services.AddAuthentication().AddJwtBearer("GatewayAuthScheme", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            
        
        ValidateIssuer = false, 
        ValidateAudience = false, 
            
        ClockSkew = TimeSpan.Zero
    };
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
app.UseAuthentication(); // Önce kimlik sor
await app.UseOcelot(); // Sonra yönlendir

app.Run();