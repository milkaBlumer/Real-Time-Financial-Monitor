using SQLink.Abstractions;
using SQLink.Models;

namespace SQLink.Services
{
    public sealed class RedisRealtimePublisher : IRealtimePublisher
    {
        private readonly IRedisMessageBroker _redisBroker;

        public RedisRealtimePublisher(IRedisMessageBroker redisBroker)
        {
            _redisBroker = redisBroker;
        }

        public Task PublishTransactionAsync(Transaction tx, CancellationToken cancellationToken = default)
            => _redisBroker.PublishTransactionAsync(tx, cancellationToken);
    }
}
