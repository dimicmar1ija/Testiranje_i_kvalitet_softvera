using ForumAPI.Models;

namespace ForumAPI.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(string id);
        Task CreateAsync(Category category);
        Task DeleteAsync(string id);
        Task<Category?> GetByNameAsync(string name);

    }
}
