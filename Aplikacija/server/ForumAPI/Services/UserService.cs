using ForumAPI.Models;
using ForumAPI.Repositories;

namespace ForumApi.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Task<List<User>> GetAllAsync() => _userRepository.GetAllAsync();

        public Task<User?> GetByIdAsync(string id) => _userRepository.GetByIdAsync(id);

        public Task UpdateAsync(User user) => _userRepository.UpdateAsync(user);

        public Task DeleteAsync(string id) => _userRepository.DeleteAsync(id.ToString());

        public Task<User?> GetByUsernameAsync(string username)
        {
            return _userRepository.GetByUsernameAsync(username);
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            return _userRepository.GetByEmailAsync(email);
        }

        public Task CreateAsync(User user)
        {
            return _userRepository.CreateAsync(user);
        }
    }
}
