using Microsoft.EntityFrameworkCore;
using SQLink.Abstractions;
using SQLink.Data;
using SQLink.Models;

namespace SQLink.Services
{
    public class TransactionDbStore : IPersistentStore
    {
        private readonly ApplicationDbContext _context;

        public TransactionDbStore(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Transaction?> GetTransactionAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Transaction>> GetRecentTransactionsAsync(int count, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .OrderByDescending(t => t.Timestamp)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}
