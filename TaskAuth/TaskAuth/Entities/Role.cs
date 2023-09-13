using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskAuth.Entities
{
    [Table("Roles")]
    [Index(nameof(RoleName), IsUnique = true)]
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } = null!;
        public ICollection<User> Users { get; } = new List<User>();
    }
}
