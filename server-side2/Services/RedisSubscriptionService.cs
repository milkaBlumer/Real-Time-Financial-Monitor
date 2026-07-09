using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using SQLink.Models;

namespace SQLink.Services
{
    public class RedisSubscriptionService : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<RedisSubscriptionService> _logger;
        private const string TransactionsChannel = "transactions_channel";

        public RedisSubscriptionService(
            IConnectionMultiplexer redis,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<RedisSubscriptionService> logger)
        {
            _redis = redis;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscriber = _redis.GetSubscriber();
            
            _logger.LogInformation("Starting Redis subscription to {Channel}", TransactionsChannel);

            // Subscribe to transactions channel
            await subscriber.SubscribeAsync(RedisChannel.Literal(TransactionsChannel), async (channel, message) =>
            {
                try
                {
                    if (!message.HasValue)
                        return;

                    var txJson = message.ToString();
                    var transaction = JsonSerializer.Deserialize<Transaction>(txJson);

                    if (transaction == null)
                    {
                        _logger.LogWarning("Failed to deserialize transaction message");
                        return;
                    }

                    _logger.LogInformation("Received transaction {TransactionId} from Redis channel", transaction.Id);

                    // Create a scope to get the SignalR hub context
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var hubContext = scope.ServiceProvider
                            .GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<TransactionHub>>();

                        // Broadcast to all connected clients on this Pod
                        await hubContext.Clients.All.SendAsync("transaction", transaction, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing Redis message");
                }
            });

            // Keep the service running until cancellation is requested
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Redis subscription service");
            await base.StopAsync(cancellationToken);
        }
    }
}
