public class PostDto
{
    public string Id { get; set; }
    //public string AuthorId { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }

    public List<Category> Tags { get; set; }

    public List<string> MediaUrls { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool isEdited { get; set; }
}