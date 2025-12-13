using ForumAPI.Models;
using MongoDB.Driver;

namespace ForumAPI.Repositories
{
    public class TestRepository : ITestRepository
    {
        private readonly IMongoCollection<TestItem> _collection;

        public TestRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<TestItem>("TestItems");
        }

        public async Task<List<TestItem>> GetAllAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        public async Task CreateAsync(TestItem item) =>
            await _collection.InsertOneAsync(item);
    }
}
