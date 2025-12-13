using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

public class Post
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string AuthorId { get; set; }

    public required string Title { get; set; }

    public required string Body { get; set; }

    // Lista URL-ova za slike i video
    public List<string> MediaUrls { get; set; } = new();

    // Lista tag ID-jeva
    public List<string> TagsIds { get; set; } = new();

    // Lista ID-jeva korisnika koji su lajkovali post
    public List<string> LikedByUserIds { get; set; } = new();

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; }

    // Automatski se računa da li je post izmenjen
    [BsonIgnore]
    public bool IsEdited => CreatedAt != UpdatedAt;
}
