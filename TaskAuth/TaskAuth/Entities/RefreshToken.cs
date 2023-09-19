using System.ComponentModel.DataAnnotations;

namespace TaskAuth.Entities
{
    // [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        [MaxLength(21)]
        public string Id { get; set; } = null!;
        [Required]
        [MaxLength]
        public string Token { get; set; } = null!;
        [Required]
        public DateTime IsUsedAt { get; set; } = DateTime.Now;
        [Required]
        public DateTime IsExpiredAt { get; set; }
        [Required]
        public bool IsRevoke { get; set; } = false;
        [Required]
        public bool IsUsed { get; set; } = true;
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public string? ParentId { get; set; }
        public RefreshToken? Parent { get; set; }
        public ICollection<RefreshToken> Children { get; } = new List<RefreshToken>();
    }
}
