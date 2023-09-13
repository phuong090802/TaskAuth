using Microsoft.EntityFrameworkCore;

namespace TaskAuth.Entities
{
    public class TaskAuthContext : DbContext
    {
        public TaskAuthContext(DbContextOptions options) : base(options) { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.ClientNoAction);

            modelBuilder.Entity<RefreshToken>()
              .HasOne(u => u.User)
              .WithOne(u => u.RefreshToken)
              .HasForeignKey<User>(r => r.RefreshTokenId)
               .OnDelete(DeleteBehavior.ClientNoAction);

            modelBuilder.Entity<Role>()
                .HasMany(u => u.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(r => r.RoleId)
                .OnDelete(DeleteBehavior.ClientNoAction);
        }
    }
}
