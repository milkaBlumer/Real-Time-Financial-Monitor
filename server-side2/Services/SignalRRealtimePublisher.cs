using Microsoft.AspNetCore.SignalR;
using SQLink.Abstractions;
using SQLink.Models;

namespace SQLink.Services
{
    public sealed class SignalRRealtimePublisher : IRealtimePublisher
    {
        private readonly IHubContext<TransactionHub> _hubContext;
        private readonly IRedisMessageBroker _redisBroker;

        public SignalRRealtimePublisher(
            IHubContext<TransactionHub> hubContext,
            IRedisMessageBroker redisBroker)
        {
            _hubContext = hubContext;
            _redisBroker = redisBroker;
        }

        public async Task PublishTransactionAsync(Transaction tx, CancellationToken cancellationToken = default)
        {
            // Publish to Redis so all Pods receive it (Pub/Sub)
            await _redisBroker.PublishTransactionAsync(tx, cancellationToken);

            // Also publish to local clients (fallback for same Pod)
            await _hubContext.Clients.All.SendAsync("transaction", tx, cancellationToken);
        }
    }
}
