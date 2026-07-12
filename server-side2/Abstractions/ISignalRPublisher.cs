using SQLink.Models;

namespace SQLink.Abstractions
{
    public interface ISignalRPublisher
    {
        Task BroadcastTransactionAsync(Transaction tx, CancellationToken cancellationToken = default);
    }
}
