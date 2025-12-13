using ForumAPI.Models;

public interface IUserRepository
{
    Task CreateAsync(User user);
    Task<List<User>> GetAllAsync();
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(string id);
    Task UpdateAsync(User user);
    Task DeleteAsync(string userId);


}

