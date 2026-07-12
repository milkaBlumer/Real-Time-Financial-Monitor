using System.Text.Json;
using StackExchange.Redis;
using SQLink.Abstractions;
using SQLink.Models;

namespace SQLink.Services
{
    public class RedisSubscriptionService : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ISignalRPublisher _signalRPublisher;
        private readonly ILogger<RedisSubscriptionService> _logger;
        private const string TransactionsChannel = "transactions_channel";

        public RedisSubscriptionService(
            IConnectionMultiplexer redis,
            ISignalRPublisher signalRPublisher,
            ILogger<RedisSubscriptionService> logger)
        {
            _redis = redis;
            _signalRPublisher = signalRPublisher;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscriber = _redis.GetSubscriber();

            _logger.LogInformation("Starting Redis subscription to {Channel}", TransactionsChannel);

            await subscriber.SubscribeAsync(RedisChannel.Literal(TransactionsChannel), async (channel, message) =>
            {
                try
                {
                    if (!message.HasValue)
                        return;

                    var transaction = JsonSerializer.Deserialize<Transaction>(message.ToString());

                    if (transaction == null)
                    {
                        _logger.LogWarning("Failed to deserialize transaction message");
                        return;
                    }

                    _logger.LogInformation("Received transaction {TransactionId} from Redis channel", transaction.Id);

                    await _signalRPublisher.BroadcastTransactionAsync(transaction, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing Redis message");
                }
            });

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Redis subscription service");
            await base.StopAsync(cancellationToken);
        }
    }
}
