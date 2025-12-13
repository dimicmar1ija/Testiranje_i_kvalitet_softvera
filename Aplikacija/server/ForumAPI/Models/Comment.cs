using MongoDB.Bson.Serialization.Attributes;

public class Comment
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id { get; set; }
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public required string PostId { get; set; }
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public required string AuthorId { get; set; }
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? ParentCommentId { get; set; }    // zbog odogovora na komentar
    public required string Body { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<string> LikedByUserIds { get; set; } = new List<string>();
    public List<string> DislikedByUserIds { get; set; } = new List<string>();
     public int LikesCount => LikedByUserIds.Count;
    public int DislikesCount => DislikedByUserIds.Count;
}