namespace TaskAuth.Models
{
    public class UserModel
    {
        public required Guid Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; } 
        public required string HashedPassword { get; set; }
        public required string Role { get; set; } 
    }
}
