using MongoDB.Bson;
using MongoDB.Driver;

namespace AuthApi.Services
{
    public class UserService
    {
        private readonly IMongoCollection<BsonDocument> _usersCollection;

        public UserService(IMongoClient mongoClient, IConfiguration configuration)
        {
            var connectionString = configuration["MongoDB:ConnectionString"];
            var databaseName = configuration["MongoDB:DatabaseName"];
            var database = mongoClient.GetDatabase(databaseName);
            Console.WriteLine($"Connecting to database: {databaseName}");

            _usersCollection = database.GetCollection<BsonDocument>("users");

        }

        public async Task CreateUserAsync(BsonDocument userDocument)
        {
            await _usersCollection.InsertOneAsync(userDocument);
        }

        public async Task<BsonDocument> GetUserAsync(FilterDefinition<BsonDocument> filter)
        {
            return await _usersCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task UpdateUserAsync(FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update)
        {
            var result = await _usersCollection.UpdateOneAsync(filter, update);
            if (result.MatchedCount == 0)
            {
                throw new Exception("User not found or update failed.");
            }
        }
    }
}
