using ForumAPI.Models;

namespace ForumAPI.Repositories
{
    public interface ITestRepository
    {
        Task<List<TestItem>> GetAllAsync();
        Task CreateAsync(TestItem item);
    }
}
