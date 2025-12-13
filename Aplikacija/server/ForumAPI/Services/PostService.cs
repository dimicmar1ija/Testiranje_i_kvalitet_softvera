namespace ForumApi.Services
{
    public class PostService
    {
        private readonly IPostRepository _repo;
        public PostService(IPostRepository repo)
        {
            _repo = repo;
        }

        public Task<Post> GetByIdAsync(string id) => _repo.GetByIdAsync(id);

        public Task<IEnumerable<Post>> GetAllAsync() => _repo.GetAllAsync();

        public Task<IEnumerable<Post>> GetByTagAsync(string tagId) => _repo.GetByTagAsync(tagId);

        public Task<IEnumerable<Post>> GetByAuthorAsync(string authorId) => _repo.GetByAuthorAsync(authorId);

        public Task CreateAsync(Post post) => _repo.CreateAsync(post);

        public Task UpdateAsync(Post post) => _repo.UpdateAsync(post);

        public async Task DeleteAsync(Post post)
        {
            await _repo.DeleteAsync(post);
        }

        public async Task LikePostAsync(string postId, string userId)
        {
            var post = await _repo.GetByIdAsync(postId);
            if (post == null) throw new KeyNotFoundException("Post nije pronađen.");

            if (post.LikedByUserIds.Contains(userId))
            {
                post.LikedByUserIds.Remove(userId); // Unlike
            }
            else
            {
                post.LikedByUserIds.Add(userId); // Like
            }

            post.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(post);
        }

        public Task<List<Post>> GetByTagsAsync(IEnumerable<string> tagIds, bool matchAll) =>
            _repo.GetByTagsAsync(tagIds, matchAll);

    }

    
}