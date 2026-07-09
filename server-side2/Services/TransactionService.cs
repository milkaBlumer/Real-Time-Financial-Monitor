using SQLink.Abstractions;
using SQLink.Contracts;
using SQLink.Models;

namespace SQLink.Services
{
    public sealed class TransactionService : ITransactionService
    {
        private readonly ITransactionStore _cache;
        private readonly IPersistentStore _database;
        private readonly IRealtimePublisher _realtimePublisher;

        public TransactionService(
            ITransactionStore cache,
            IPersistentStore database,
            IRealtimePublisher realtimePublisher)
        {
            _cache = cache;
            _database = database;
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

            // 1. Save to persistent database first (Source of Truth)
            await _database.SaveTransactionAsync(tx, cancellationToken);

            // 2. Update cache for fast access
            _cache.Upsert(tx);

            // 3. Publish to connected clients in real-time
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
            return _cache.GetAll();
        }
    }
}
