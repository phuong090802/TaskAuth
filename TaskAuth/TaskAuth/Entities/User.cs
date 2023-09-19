using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TaskAuth.Entities
{
    // [Table("Users")]
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        [MaxLength(21)]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)] // it inactive
        public string Id { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;
        
        [Required]
        [MaxLength(254)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(62)]
        public string HashedPassword { get; set; } = null!;

        public int RoleId { get; set; }

        public Role Role { get; set; } = null!;
        public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();
    }
}

