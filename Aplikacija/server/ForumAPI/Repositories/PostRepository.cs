using MongoDB.Driver;

public class PostRepository : IPostRepository
{
    private readonly IMongoCollection<Post> _posts;

    public PostRepository(IMongoDatabase database)
    {
        _posts = database.GetCollection<Post>("Posts");
       
        _posts.Indexes.CreateOne(new CreateIndexModel<Post>(
            Builders<Post>.IndexKeys.Ascending(p => p.TagsIds)));
    }

    public async Task CreateAsync(Post post)
    {
        if (post == null)
            throw new ArgumentNullException(nameof(post));

        await _posts.InsertOneAsync(post);
    }

    public async Task DeleteAsync(Post post)
    {
        if (post == null || string.IsNullOrEmpty(post.Id))
            throw new ArgumentException("Post ili Id nije validan");

        await _posts.DeleteOneAsync(p => p.Id == post.Id);
    }

    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await _posts.Find(_ => true).ToListAsync();
    }

    public async Task<Post> GetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Id nije validan");

        return await _posts.Find(post => post.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Post>> GetByAuthorAsync(string authorId)
    {
        if (string.IsNullOrEmpty(authorId))
            throw new ArgumentException("authorId nije validan");

        return await _posts.Find(post => post.AuthorId == authorId).ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetByTagAsync(string tagId)
    {
        if (string.IsNullOrEmpty(tagId))
            throw new ArgumentException("tagId nije validan");

        return await _posts.Find(post => post.TagsIds != null && post.TagsIds.Contains(tagId)).ToListAsync();
    }

    public async Task UpdateAsync(Post post)
    {
        if (post == null || string.IsNullOrEmpty(post.Id))
            throw new ArgumentException("Post ili Id nije validan");

        await _posts.ReplaceOneAsync(p => p.Id == post.Id, post);
    }

    public async Task ToggleLikeAsync(string postId, string userId)
    {
        var post = await GetByIdAsync(postId);
        if (post == null) throw new KeyNotFoundException();

        var update = post.LikedByUserIds.Contains(userId)
            ? Builders<Post>.Update.Pull(p => p.LikedByUserIds, userId)
            : Builders<Post>.Update.Push(p => p.LikedByUserIds, userId);

        update = update.Set(p => p.UpdatedAt, DateTime.UtcNow);
        await _posts.UpdateOneAsync(p => p.Id == postId, update);
    }

    public async Task<List<Post>> GetByTagsAsync(IEnumerable<string> tagIds, bool matchAll)
    {
        var ids = tagIds.ToList();
        var filter = matchAll
            ? Builders<Post>.Filter.All(p => p.TagsIds, ids)       // svi izabrani tagovi
            : Builders<Post>.Filter.AnyIn(p => p.TagsIds, ids);     // bar jedan tag

        return await _posts.Find(filter)
                        .SortByDescending(p => p.CreatedAt)
                        .ToListAsync();
    }

}
