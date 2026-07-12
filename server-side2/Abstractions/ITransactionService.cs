using SQLink.Models;

namespace SQLink.Abstractions
{
    public interface ITransactionService
    {
        Task<Transaction> IngestAsync(Transaction request, CancellationToken cancellationToken = default);
        IEnumerable<Transaction> GetAll();
    }
}
