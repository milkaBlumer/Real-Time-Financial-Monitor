using SQLink.Contracts;
using SQLink.Models;

namespace SQLink.Abstractions
{
    public interface ITransactionService
    {
        //TransactionService(ITransactionStore store, IRealtimePublisher realtimePublisher);
        Task<TransactionResponse> IngestAsync(TransactionRequest request, CancellationToken cancellationToken = default);
        IEnumerable<Transaction> GetAll();
        // void Upsert(Transaction tx);
        // IEnumerable<Transaction> GetAll();
        // Transaction? Get(string id);
    }
}
