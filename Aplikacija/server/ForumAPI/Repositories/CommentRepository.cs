using ForumAPI.Models;
using MongoDB.Driver;

namespace ForumAPI.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IMongoCollection<Comment> _comments;

        public CommentRepository(IMongoDatabase database)
        {
            _comments = database.GetCollection<Comment>("Comments");

            _comments.Indexes.CreateOne(
                new CreateIndexModel<Comment>(
                    Builders<Comment>.IndexKeys.Ascending(c => c.ParentCommentId)));
        }

        public async Task<List<Comment>> GetByPostIdAsync(string postId)
        {
            return await _comments.Find(c => c.PostId == postId).ToListAsync();
        }

        public async Task<Comment> GetByIdAsync(string id)
        {
            return await _comments.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Comment comment)
        {
            await _comments.InsertOneAsync(comment);
        }

        public async Task UpdateAsync(Comment comment)
        {
            await _comments.ReplaceOneAsync(c => c.Id == comment.Id, comment);
        }

        public async Task DeleteAsync(string id)
        {
            await _comments.DeleteOneAsync(c => c.Id == id);
        }

          public async Task<List<string>> GetChildrenIdsAsync(string parentId)
        {
            var filter = Builders<Comment>.Filter.Eq(c => c.ParentCommentId, parentId);
            return await _comments.Find(filter).Project(c => c.Id).ToListAsync();
        }

        public Task DeleteManyByIdsAsync(IEnumerable<string> ids)
        {
            var filter = Builders<Comment>.Filter.In(c => c.Id, ids);
            return _comments.DeleteManyAsync(filter);
        }
    }
}
