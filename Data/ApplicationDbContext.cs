using Microsoft.EntityFrameworkCore;
using IntegradorIdeas.Models;

namespace IntegradorIdeas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Idea> Ideas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique(); // El nombre de equipo no puede repetirse
            });
        }
    }
}
