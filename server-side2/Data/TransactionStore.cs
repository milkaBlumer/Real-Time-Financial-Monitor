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

        public TransactionStore(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = redis.GetDatabase();
        }

        public void Upsert(Transaction tx)
        {
            var json = JsonSerializer.Serialize(tx);
            var key = $"{TransactionKeyPrefix}{tx.Id}";
            _db.StringSet(key, json);
        }

        public IEnumerable<Transaction> GetAll()
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{TransactionKeyPrefix}*");
            
            var transactions = new List<Transaction>();
            foreach (var key in keys)
            {
                var json = _db.StringGet(key);
                if (json.HasValue)
                {
                    var tx = JsonSerializer.Deserialize<Transaction>(json.ToString());
                    if (tx != null)
                    {
                        transactions.Add(tx);
                    }
                }
            }
            
            return transactions.OrderByDescending(t => t.Timestamp);
        }

        // public Transaction? Get(string id)
        // {
        //     var key = $"{TransactionKeyPrefix}{id}";
        //     var json = _db.StringGet(key);
            
        //     if (!json.HasValue)
        //         return null;
            
        //     return JsonSerializer.Deserialize<Transaction>(json.ToString());
        // }
    }
}
