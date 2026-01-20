var builder = WebApplication.CreateBuilder(args);

// Ayarları "appsettings.json" dosyasındaki "ReverseProxy" bölümünden alacak.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();

app.Run();