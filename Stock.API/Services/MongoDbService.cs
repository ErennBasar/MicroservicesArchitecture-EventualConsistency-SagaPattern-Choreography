using MongoDB.Driver;

namespace Stock.API.Services;

public class MongoDbService
{
    public IMongoDatabase Database { get; }
    public IMongoClient Client { get; }

    public MongoDbService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDb");
        var mongoUrl = MongoUrl.Create(connectionString);
        
        //MongoClient client = new(configuration.GetConnectionString("MongoDb")); (Eski hâli)
        Client = new MongoClient(mongoUrl);
        Database = Client.GetDatabase("StockApiDb");
    }

    public IMongoCollection<T> GetCollection<T>() => Database.GetCollection<T>(typeof(T)
        .Name.ToLowerInvariant());
}