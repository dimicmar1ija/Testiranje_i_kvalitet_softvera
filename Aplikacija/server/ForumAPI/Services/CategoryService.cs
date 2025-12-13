using ForumAPI.Models;
using ForumAPI.Repositories;

namespace ForumAPI.Services
{
    public class CategoryService
    {
        private readonly ICategoryRepository _repo;

        public CategoryService(ICategoryRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Category>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Category> GetByIdAsync(string id) => _repo.GetByIdAsync(id);
        public Task CreateAsync(Category cat) => _repo.CreateAsync(cat);
        public Task DeleteAsync(string id) => _repo.DeleteAsync(id);
       public Task<Category?> GetByNameAsync(string name)
        {
            return _repo.GetByNameAsync(name);
        }

    }
}
