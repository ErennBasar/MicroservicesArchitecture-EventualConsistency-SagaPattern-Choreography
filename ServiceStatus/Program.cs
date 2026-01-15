var builder = WebApplication.CreateBuilder(args);

// UI Servisini ve InMemory depolamayı ekle
builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

var app = builder.Build();

// Arayüzü /health-ui adresinde çalıştır
app.MapHealthChecksUI(options => options.UIPath = "/health-ui");

app.Run();