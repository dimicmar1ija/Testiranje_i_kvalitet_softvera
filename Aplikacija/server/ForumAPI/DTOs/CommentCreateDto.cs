using System.ComponentModel.DataAnnotations;

public class CommentCreateDto
{
    [Required]
    public string PostId { get; set; } = string.Empty;

    [Required]
    public string AuthorId { get; set; } = string.Empty;

    public string? ParentCommentId { get; set; }

    [Required]
    public string Body { get; set; } = string.Empty;
}
