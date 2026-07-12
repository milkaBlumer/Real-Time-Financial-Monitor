using Microsoft.EntityFrameworkCore;
using SQLink.Abstractions;
using SQLink.Models;

namespace SQLink.Data
{
    public class TransactionDbContext : DbContext, ITransactionRepository
    {
        public TransactionDbContext(DbContextOptions<TransactionDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; } = null!;

        public void Add(Transaction transaction)
        {
            Transactions.Add(transaction);
        }

        public IEnumerable<Transaction> GetRecent(int limit)
        {
            if (limit <= 0)
            {
                return Enumerable.Empty<Transaction>();
            }

            return Transactions
                .AsNoTracking()
                .OrderByDescending(t => t.Timestamp)
                .Take(limit)
                .ToList();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

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
            });
        }
    }
}
