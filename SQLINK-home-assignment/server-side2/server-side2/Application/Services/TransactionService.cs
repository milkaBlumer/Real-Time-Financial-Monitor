using SQLink.Abstractions;
using SQLink.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SQLink.Services
{
    public sealed class TransactionService : ITransactionService
    {
        private const int RecentReadLimit = 1000;
        private readonly ITransactionStore _cache;
        private readonly ITransactionRepository _database;
        private readonly IRealtimePublisher _realtimePublisher;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionStore cache,
            ITransactionRepository database,
            IRealtimePublisher realtimePublisher,
            ILogger<TransactionService> logger)
        {
            _cache = cache;
            _database = database;
            _realtimePublisher = realtimePublisher;
            _logger = logger;
        }

        public async Task<Transaction> IngestAsync(Transaction request, CancellationToken cancellationToken = default)
        {
            var normalizedId = request.Id.Trim();
            var normalizedCurrency = request.Currency.Trim().ToUpperInvariant();
            var normalizedStatus = request.Status.Trim();

            var tx = new Transaction
            {
                Id = normalizedId,
                Amount = request.Amount,
                Currency = normalizedCurrency,
                Status = normalizedStatus,
                Timestamp = DateTime.UtcNow
            };

            // 1. Save to persistent database first (Source of Truth)
            try
            {
                _database.Add(tx);
                await _database.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqliteException sqliteEx && sqliteEx.SqliteErrorCode == 19)
            {
                throw new TransactionConflictException(tx.Id, ex);
            }

            // 2. Update cache for fast access
            _cache.Upsert(tx);

            // 3. Publish to connected clients in real-time
            try
            {
                await _realtimePublisher.PublishTransactionAsync(tx, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish transaction {TransactionId} to realtime layer", tx.Id);
                throw new RealtimePublishException(tx.Id, ex);
            }

            return tx;
        }

        public IEnumerable<Transaction> GetAll()
        {
            try
            {
                var cachedTransactions = _cache.GetAll().ToList();
                if (cachedTransactions.Count > 0)
                {
                    return cachedTransactions;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache read failed. Falling back to database.");
            }

            var recentFromDatabase = _database.GetRecent(RecentReadLimit).ToList();

            foreach (var transaction in recentFromDatabase)
            {
                try
                {
                    _cache.Upsert(transaction);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cache backfill failed for transaction {TransactionId}", transaction.Id);
                }
            }

            return recentFromDatabase;
        }
    }
}
