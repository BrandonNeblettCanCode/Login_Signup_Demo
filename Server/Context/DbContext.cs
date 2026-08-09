using Microsoft.EntityFrameworkCore;
using Server.Model;

namespace Server.Context
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options) : base(options){}

        public DbSet<Users> Users => Set<Users>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(20);
                entity.Property(u => u.Password).IsRequired().HasMaxLength(100);
            });
        }
    }
}