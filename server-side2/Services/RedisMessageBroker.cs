using System.Text.Json;
using StackExchange.Redis;
using SQLink.Abstractions;
using SQLink.Models;

namespace SQLink.Services
{
    public class RedisMessageBroker : IRedisMessageBroker
    {
        private readonly IConnectionMultiplexer _redis;
        private const string TransactionsChannel = "transactions_channel";

        public RedisMessageBroker(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task PublishTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            var subscriber = _redis.GetSubscriber();
            var message = JsonSerializer.Serialize(transaction);
            
            // Publish to all subscribers (Pods) listening on this channel
            await subscriber.PublishAsync(RedisChannel.Literal(TransactionsChannel), message);
        }
    }
}
