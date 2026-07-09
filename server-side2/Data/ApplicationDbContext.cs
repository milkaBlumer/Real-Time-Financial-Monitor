using Microsoft.EntityFrameworkCore;
using SQLink.Models;

namespace SQLink.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Transaction entity
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasMaxLength(36)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Account)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Amount)
                    .HasPrecision(18, 2);

                entity.Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(3);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Timestamp)
                    .IsRequired();

                // Create index for faster queries
                entity.HasIndex(e => e.Timestamp).IsDescending();
                entity.HasIndex(e => e.Account);
            });
        }
    }
}
