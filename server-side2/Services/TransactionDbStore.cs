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
    }
}
