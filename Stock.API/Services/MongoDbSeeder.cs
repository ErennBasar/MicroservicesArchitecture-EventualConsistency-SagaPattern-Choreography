using MongoDB.Driver;

namespace Stock.API.Services;

public class MongoDbSeeder
{
    public static async Task Seed(IMongoCollection<Stock.API.Models.Stock> stockCollection)
    {
        var exist = await (await stockCollection.FindAsync(s => true)).AnyAsync();
        if (!exist)
        {
            await stockCollection.InsertManyAsync(GetPreconfiguredStocks());
            Console.WriteLine("SEED DATA: Stok verileri eklendi!");
        }
    }

    private static List<Stock.API.Models.Stock> GetPreconfiguredStocks()
    {
        return new List<Models.Stock>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.Parse("c4568019-2166-4e58-8686-350711993421"), // Ürün 1
                Count = 100
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.Parse("d3454321-2166-4e58-8686-350711993422"), // Ürün 2
                Count = 50
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.Parse("e1231231-2166-4e58-8686-350711993423"), // Ürün 3
                Count = 10
            }
        };
    }
}