namespace ForumAPI.Dtos
{
    public class UpdateUserDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }

        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
    }   
}
