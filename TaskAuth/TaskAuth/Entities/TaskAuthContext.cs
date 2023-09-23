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
            // convert enum to string storeage in database
            modelBuilder.Entity<Role>()
                .Property(r => r.RoleName)
                .HasConversion<string>();

            // parent - children relationship refresh token
            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.Parent)
                .WithMany(r => r.Children)
                .HasForeignKey(r => r.ParentToken)
                .OnDelete(DeleteBehavior.ClientNoAction);

            // one to many relationship (user - refresh token)
            modelBuilder.Entity<User>()
                .HasMany(r => r.RefreshTokens)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId);


            // one to many relationship (role - user)
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Users)
                .WithOne(r => r.Role)
                .HasForeignKey(r => r.RoleId)
                .OnDelete(DeleteBehavior.ClientNoAction);

        }
    }
}
