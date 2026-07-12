using Microsoft.AspNetCore.SignalR;
using SQLink.Abstractions;
using SQLink.Models;

namespace SQLink.Services
{
    public sealed class SignalRPublisher : ISignalRPublisher
    {
        private readonly IHubContext<TransactionHub> _hubContext;

        public SignalRPublisher(IHubContext<TransactionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task BroadcastTransactionAsync(Transaction tx, CancellationToken cancellationToken = default)
            => _hubContext.Clients.All.SendAsync("transaction", tx, cancellationToken);
    }
}
