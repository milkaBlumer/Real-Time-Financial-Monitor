using SQLink.Models;

namespace SQLink.Abstractions
{
    public interface IPersistentStore
    {
        Task SaveTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
        Task<Transaction?> GetTransactionAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Transaction>> GetRecentTransactionsAsync(int count, CancellationToken cancellationToken = default);
    }
}
