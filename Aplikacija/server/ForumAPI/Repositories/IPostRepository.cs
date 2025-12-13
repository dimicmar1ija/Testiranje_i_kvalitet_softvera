public interface IPostRepository
{
    Task<Post> GetByIdAsync(string id);
    Task<IEnumerable<Post>> GetAllAsync();
    Task<IEnumerable<Post>> GetByTagAsync(string tagId);
    Task<IEnumerable<Post>> GetByAuthorAsync(string authorId);
    Task CreateAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Post post);
    Task<List<Post>> GetByTagsAsync(IEnumerable<string> tagIds, bool matchAll);
}