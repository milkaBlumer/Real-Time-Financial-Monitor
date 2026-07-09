using Microsoft.AspNetCore.SignalR;
using SQLink.Models;

namespace SQLink
{
    public class TransactionHub : Hub
    {
        private readonly ConnectedClientTracker _clients;

        public TransactionHub(ConnectedClientTracker clients)
        {
            _clients = clients;
        }

        public override Task OnConnectedAsync()
        {
            _clients.Add(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _clients.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task BroadcastTransaction(Transaction tx)
        {
            await Clients.All.SendAsync("transaction", tx);
        }
    }
}
