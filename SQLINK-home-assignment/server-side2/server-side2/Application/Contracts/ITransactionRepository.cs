using SQLink.Models;

namespace SQLink.Abstractions
{
    public interface ITransactionRepository
    {
        void Add(Transaction transaction);
        IEnumerable<Transaction> GetRecent(int limit);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}