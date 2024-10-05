using MongoDB.Driver;

namespace E_Commerce_Application___ASP.NET_MongoDB.Interfaces
{
    public interface IMongoDbService
    {
        IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}
