using ForumAPI.Models;
using MongoDB.Driver;

namespace ForumAPI.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IMongoCollection<Category> _categories;

        public CategoryRepository(IMongoDatabase database)
        {
            _categories = database.GetCollection<Category>("Categories");
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _categories.Find(_ => true).ToListAsync();
        }

        public async Task<Category> GetByIdAsync(string id)
        {
            return await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Category category)
        {
            await _categories.InsertOneAsync(category);
        }

        public async Task DeleteAsync(string id)
        {
            await _categories.DeleteOneAsync(c => c.Id == id);
        }
        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _categories.Find(c => c.Name.ToLower() == name.ToLower()).FirstOrDefaultAsync();
        }

    }
}
