using SQLink.Models;

namespace SQLink.Abstractions
{
    public interface IRedisMessageBroker
    {
        Task PublishTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
    }
}
