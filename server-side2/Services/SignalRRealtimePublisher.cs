using Microsoft.AspNetCore.SignalR;
using SQLink.Abstractions;
using SQLink.Models;

namespace SQLink.Services
{
    public sealed class SignalRRealtimePublisher : IRealtimePublisher
    {
        private readonly IHubContext<TransactionHub> _hubContext;

        public SignalRRealtimePublisher(IHubContext<TransactionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PublishTransactionAsync(Transaction tx, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.All.SendAsync("transaction", tx, cancellationToken);
        }
    }
}
