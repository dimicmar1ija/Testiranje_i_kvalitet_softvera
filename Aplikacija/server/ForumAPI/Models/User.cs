using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ForumAPI.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; } = null!;

        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = null!;

        [BsonElement("role")]
        public string Role { get; set; } = "user";

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("bio")]
        public string Bio { get; set; } = "Nema biografije";

        [BsonElement("avatarUrl")]
        public string AvatarUrl { get; set; } = "https://example.com/default-avatar.png";


        
    }
}
