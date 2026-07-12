using SQLink.Models;

namespace SQLink.Abstractions
{
    public interface IRealtimePublisher
    {
        Task PublishTransactionAsync(Transaction tx, CancellationToken cancellationToken = default);
    }
}
