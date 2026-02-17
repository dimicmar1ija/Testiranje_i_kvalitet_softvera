using ForumAPI.Models;
using ForumAPI.Repositories;

namespace ForumAPI.Services
{
    public class ThreadedComment
    {
        public Comment Comment { get; set; }
        public List<ThreadedComment> Replies { get; set; } = new();
    }

    public class CommentService
    {
        private readonly ICommentRepository _repo;

        public CommentService(ICommentRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<ThreadedComment>> GetThreadedCommentsForPost(string postId)
        {
            var all = await _repo.GetByPostIdAsync(postId);
            var lookup = all.ToDictionary(c => c.Id, c => new ThreadedComment { Comment = c });

            List<ThreadedComment> roots = new();

            foreach (var c in all)
            {
                if (string.IsNullOrEmpty(c.ParentCommentId))
                    roots.Add(lookup[c.Id]);
                else if (lookup.ContainsKey(c.ParentCommentId))
                    lookup[c.ParentCommentId].Replies.Add(lookup[c.Id]);
            }

            return roots;
        }

        public async Task CreateComment(Comment comment)
        {
            await _repo.CreateAsync(comment);
        }

        public async Task<Comment?> GetByIdAsync(string id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public Task DeleteAsync(string commentId)
        {
            return _repo.DeleteAsync(commentId);
        }


        public async Task UpdateComment(Comment comment)
        {
            comment.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(comment);
        }

        public async Task DeleteComment(string id)
        {
            await _repo.DeleteAsync(id);
        }

        public async Task<bool> LikeComment(string commentId, string userId)
        {
            var comment = await _repo.GetByIdAsync(commentId);
            if (comment == null) return false;

            // Ako korisnik već nije lajkovao
            if (!comment.LikedByUserIds.Contains(userId))
            {
                comment.LikedByUserIds.Add(userId);

                // Ako je ranije dislajkovao, ukloni ga iz te liste
                if (comment.DislikedByUserIds.Contains(userId))
                    comment.DislikedByUserIds.Remove(userId);

                await _repo.UpdateAsync(comment);
                return true;
            }
            return false; // Već je lajkovao, nema dupliranja
        }

        public async Task<bool> DislikeComment(string commentId, string userId)
        {
            var comment = await _repo.GetByIdAsync(commentId);
            if (comment == null) return false;

            // Ako korisnik već nije dislajkovao
            if (!comment.DislikedByUserIds.Contains(userId))
            {
                comment.DislikedByUserIds.Add(userId);

                // Ako je ranije lajkovao, ukloni ga iz te liste
                if (comment.LikedByUserIds.Contains(userId))
                    comment.LikedByUserIds.Remove(userId);

                await _repo.UpdateAsync(comment);
                return true;
            }
            return false; // Već je dislajkovao
        }

        public async Task<bool> UnlikeComment(string commentId, string userId)
        {
            var comment = await _repo.GetByIdAsync(commentId);
            if (comment == null) return false;

            if (comment.LikedByUserIds.Contains(userId))
            {
                comment.LikedByUserIds.Remove(userId);
                await _repo.UpdateAsync(comment);
                return true;
            }
            return false; // Nije lajkovao, pa ne može da "unlike"
        }

        public async Task<bool> UndislikeComment(string commentId, string userId)
        {
            var comment = await _repo.GetByIdAsync(commentId);
            if (comment == null) return false;

            if (comment.DislikedByUserIds.Contains(userId))
            {
                comment.DislikedByUserIds.Remove(userId);
                await _repo.UpdateAsync(comment);
                return true;
            }
            return false; // Nije dislajkovao, pa ne može da "undislike"
        }
        
        public async Task DeleteCommentTreeAsync(string rootId)
        {
            var toDelete = new List<string> { rootId };
            var q = new Queue<string>();
            q.Enqueue(rootId);

            while (q.Count > 0)
            {
                var current = q.Dequeue();
                var children = await _repo.GetChildrenIdsAsync(current);

                foreach (var childId in children)
                {
                    toDelete.Add(childId);
                    q.Enqueue(childId);
                }
            }

            await _repo.DeleteManyByIdsAsync(toDelete);
        }

         public async Task DeleteAllByPostIdAsync(string postId)
        {
            await _repo.DeleteManyByPostIdAsync(postId);
        }
    }
}
