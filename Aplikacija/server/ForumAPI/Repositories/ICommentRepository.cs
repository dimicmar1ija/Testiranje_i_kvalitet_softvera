using ForumAPI.Models;

namespace ForumAPI.Repositories
{
    public interface ICommentRepository
    {
        Task<List<Comment>> GetByPostIdAsync(string postId);
        Task<Comment> GetByIdAsync(string id);
        Task CreateAsync(Comment comment);
        Task UpdateAsync(Comment comment);
        Task DeleteAsync(string id);
        Task<List<string>> GetChildrenIdsAsync(string parentId);
        Task DeleteManyByIdsAsync(IEnumerable<string> ids);
    }
}
