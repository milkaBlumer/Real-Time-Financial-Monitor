using SQLink.Models;

namespace SQLink.Abstractions
{
    public interface IPersistentStore
    {
        Task SaveTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
    }
}
