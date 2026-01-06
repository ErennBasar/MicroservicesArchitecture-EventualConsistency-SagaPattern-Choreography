using MongoDB.Driver;

namespace Stock.API.Services;

public class MongoDbService
{
    private readonly IMongoDatabase _database;

    public MongoDbService(IConfiguration configuration)
    {
        MongoClient client = new(configuration.GetConnectionString("MongoDb"));
        _database = client.GetDatabase("StockApiDb");
    }
}