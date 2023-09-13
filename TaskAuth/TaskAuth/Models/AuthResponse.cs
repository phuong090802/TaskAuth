namespace TaskAuth.Models
{
    public class AuthResponse
    {
        public required Guid Id { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public required string Token { get; set; }
    }
}
