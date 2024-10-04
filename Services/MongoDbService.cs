using E_Commerce_Application___ASP.NET_MongoDB.Helpers;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace E_Commerce_Application___ASP.NET_MongoDB.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IOptions<MongoDbSettings> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            _database = client.GetDatabase(options.Value.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}
