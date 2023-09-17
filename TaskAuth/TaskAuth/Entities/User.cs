using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TaskAuth.Entities
{
    // [Table("Users")]
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

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

        public int? RefreshTokenId { get; set; }

        public Role Role { get; set; } = null!;

        public RefreshToken? RefreshToken { get; set; }
    }
}

