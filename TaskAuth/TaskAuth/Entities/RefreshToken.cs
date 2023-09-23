using System.ComponentModel.DataAnnotations;

namespace TaskAuth.Entities
{
    // [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        [MaxLength(21)]
        public string Token { get; set; } = null!;
        
        [Required]
        public DateTime IsExpiredAt { get; set; }
        
        [Required]
        public bool IsUsed { get; set; } = true;

        [Required]
        public string UserId { get; set; } = null!;
        
        [Required]
        public User User { get; set; } = null!;
        
        [MaxLength(21)]
        public string? ParentToken { get; set; }
        
        public RefreshToken? Parent { get; set; }
        
        public ICollection<RefreshToken> Children { get; } = new List<RefreshToken>();
    }
}
