public record UserDto
{
  public required string Id { get; set; }
  public required string Username { get; set; }
  public required string Email { get; set; }
  public required string Role { get; set; }
  public DateTime CreatedAt { get; set; }
  public string? Bio { get; set; }
  public string? AvatarUrl { get; set; }
}


public record UserPreviewDto
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string  AvatarUrl { get; set; }
}