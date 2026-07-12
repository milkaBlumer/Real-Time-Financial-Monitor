using System.Linq;
using System.Text.Json;
using StackExchange.Redis;
using SQLink.Abstractions;
using SQLink.Models;

namespace SQLink.Services
{
    public class TransactionStore : ITransactionStore
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private const string TransactionKeyPrefix = "transaction:";
        private const string RecentTransactionsKey = "transactions:recent";
        private const int MaxTransactions = 1000;
        private static readonly TimeSpan TransactionTtl = TimeSpan.FromHours(1);

        public TransactionStore(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = redis.GetDatabase();
        }

        public void Upsert(Transaction tx)
        {
            var json = JsonSerializer.Serialize(tx);
            var key = $"{TransactionKeyPrefix}{tx.Id}";
            var score = new DateTimeOffset(tx.Timestamp.ToUniversalTime()).ToUnixTimeMilliseconds();

            _db.StringSet(key, json, TransactionTtl);
            _db.SortedSetAdd(RecentTransactionsKey, tx.Id, score);

            var overflowIds = _db.SortedSetRangeByRank(
                RecentTransactionsKey,
                0,
                -MaxTransactions - 1,
                Order.Ascending);

            if (overflowIds.Length > 0)
            {
                _db.SortedSetRemove(RecentTransactionsKey, overflowIds);

                foreach (var overflowId in overflowIds)
                {
                    if (overflowId.IsNullOrEmpty)
                    {
                        continue;
                    }

                    _db.KeyDelete($"{TransactionKeyPrefix}{overflowId!}");
                }
            }

            _db.SortedSetRemoveRangeByScore(
                RecentTransactionsKey,
                double.NegativeInfinity,
                DateTimeOffset.UtcNow.Add(-TransactionTtl).ToUnixTimeMilliseconds());
        }

        public IEnumerable<Transaction> GetAll()
        {
            var transactions = new List<Transaction>();

            var ids = _db.SortedSetRangeByRank(
                RecentTransactionsKey,
                0,
                MaxTransactions - 1,
                Order.Descending);

            foreach (var id in ids)
            {
                if (id.IsNullOrEmpty)
                {
                    continue;
                }

                var json = _db.StringGet($"{TransactionKeyPrefix}{id!}");
                if (json.HasValue)
                {
                    var tx = JsonSerializer.Deserialize<Transaction>(json.ToString());
                    if (tx != null)
                    {
                        transactions.Add(tx);
                    }
                }
                else
                {
                    _db.SortedSetRemove(RecentTransactionsKey, id);
                }
            }

            return transactions.OrderByDescending(t => t.Timestamp);
        }
    }
}
