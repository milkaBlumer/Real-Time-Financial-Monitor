using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SQLink.Abstractions;
using SQLink.Data;
using SQLink.Models;

namespace server_side2.Tests.Integration;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath;
    private readonly string _connectionString;

    public TestWebApplicationFactory()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"sqlink-tests-{Guid.NewGuid():N}.db");
        _connectionString = $"Data Source={_databasePath};Cache=Shared";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TransactionDbContext>));
            if (dbDescriptor != null)
            {
                services.Remove(dbDescriptor);
            }

            var repoDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ITransactionRepository));
            if (repoDescriptor != null)
            {
                services.Remove(repoDescriptor);
            }

            var storeDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ITransactionStore));
            if (storeDescriptor != null)
            {
                services.Remove(storeDescriptor);
            }

            var realtimeDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IRealtimePublisher));
            if (realtimeDescriptor != null)
            {
                services.Remove(realtimeDescriptor);
            }

            services.AddDbContext<TransactionDbContext>(options =>
                options.UseSqlite(_connectionString));

            services.AddScoped<ITransactionRepository>(sp =>
                sp.GetRequiredService<TransactionDbContext>());

            services.AddSingleton<ITransactionStore, InMemoryTransactionStore>();
            services.AddSingleton<IRealtimePublisher, NoOpRealtimePublisher>();

            var hostedServices = services
                .Where(d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService))
                .ToList();
            foreach (var hostedService in hostedServices)
            {
                if (hostedService.ImplementationType?.Name == "RedisSubscriptionService")
                {
                    services.Remove(hostedService);
                }
            }

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            TryDelete(_databasePath);
            TryDelete($"{_databasePath}-shm");
            TryDelete($"{_databasePath}-wal");
        }
    }

    private static void TryDelete(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // Ignore teardown cleanup failures in tests.
        }
    }

    private sealed class NoOpRealtimePublisher : IRealtimePublisher
    {
        public Task PublishTransactionAsync(Transaction tx, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class InMemoryTransactionStore : ITransactionStore
    {
        private readonly object _gate = new();
        private readonly Dictionary<string, Transaction> _items = new();

        public void Upsert(Transaction tx)
        {
            lock (_gate)
            {
                _items[tx.Id] = tx;
            }
        }

        public IEnumerable<Transaction> GetAll()
        {
            lock (_gate)
            {
                return _items.Values
                    .OrderByDescending(x => x.Timestamp)
                    .ToList();
            }
        }
    }
}
