using SQLink.Abstractions;
using SQLink.Contracts;
using SQLink.Models;

namespace SQLink.Services
{
    public sealed class TransactionService : ITransactionService
    {
        private readonly ITransactionStore _store;
        private readonly IRealtimePublisher _realtimePublisher;

        public TransactionService(ITransactionStore store, IRealtimePublisher realtimePublisher)
        {
            _store = store;
            _realtimePublisher = realtimePublisher;
        }

        public async Task<TransactionResponse> IngestAsync(TransactionRequest request, CancellationToken cancellationToken = default)
        {
            var tx = new Transaction
            {
                Account = request.Account,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = request.Status,
                Timestamp = DateTime.UtcNow
            };

            _store.Upsert(tx);
            await _realtimePublisher.PublishTransactionAsync(tx, cancellationToken);

            return new TransactionResponse
            {
                Id = tx.Id,
                Account = tx.Account,
                Amount = tx.Amount,
                Currency = tx.Currency,
                Timestamp = tx.Timestamp,
                Status = tx.Status
            };
        }

        public IEnumerable<Transaction> GetAll()
        {
            return _store.GetAll();
        }
    }
}
