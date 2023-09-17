using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskAuth.Entities
{
    // [Table("Roles")]
    [Index(nameof(RoleName), IsUnique = true)]
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public RoleName RoleName { get; set; }
        
        public ICollection<User> Users { get; } = new List<User>();
    }

    public enum RoleName
    {
        user,
        admin
    }
}
