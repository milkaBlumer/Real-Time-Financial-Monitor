using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SQLink.Abstractions;
using SQLink.Models;
using SQLink.Services;
using Xunit;

namespace server_side2.Tests;

public class TransactionServiceTests
{
    [Fact]
    public async Task IngestAsync_Persists_Caches_And_Publishes()
    {
        var cache = new Mock<ITransactionStore>();
        var repository = new Mock<ITransactionRepository>();
        var realtime = new Mock<IRealtimePublisher>();
        var logger = new Mock<ILogger<TransactionService>>();

        var sut = new TransactionService(cache.Object, repository.Object, realtime.Object, logger.Object);

        var request = new Transaction
        {
            Id = " tx-1 ",
            Amount = 42.2m,
            Currency = " usd ",
            Status = "Completed",
            Timestamp = DateTime.UtcNow.AddDays(-1)
        };

        var result = await sut.IngestAsync(request);

        result.Id.Should().Be("tx-1");
        result.Currency.Should().Be("USD");
        result.Status.Should().Be("Completed");

        repository.Verify(r => r.Add(It.IsAny<Transaction>()), Times.Once);
        repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(c => c.Upsert(It.IsAny<Transaction>()), Times.Once);
        realtime.Verify(r => r.PublishTransactionAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IngestAsync_WhenRealtimeFails_ThrowsRealtimePublishException()
    {
        var cache = new Mock<ITransactionStore>();
        var repository = new Mock<ITransactionRepository>();
        var realtime = new Mock<IRealtimePublisher>();
        var logger = new Mock<ILogger<TransactionService>>();

        realtime
            .Setup(r => r.PublishTransactionAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("publish failed"));

        var sut = new TransactionService(cache.Object, repository.Object, realtime.Object, logger.Object);

        var request = new Transaction
        {
            Id = "tx-2",
            Amount = 14m,
            Currency = "USD",
            Status = "Pending"
        };

        await Assert.ThrowsAsync<RealtimePublishException>(() => sut.IngestAsync(request));

        repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(c => c.Upsert(It.IsAny<Transaction>()), Times.Once);
    }

    [Fact]
    public void GetAll_ReturnsCache_WhenCacheHasData()
    {
        var cache = new Mock<ITransactionStore>();
        var repository = new Mock<ITransactionRepository>();
        var realtime = new Mock<IRealtimePublisher>();
        var logger = new Mock<ILogger<TransactionService>>();

        cache.Setup(c => c.GetAll()).Returns(new[]
        {
            new Transaction { Id = "cached-1", Amount = 10m, Currency = "USD", Status = "Pending", Timestamp = DateTime.UtcNow }
        });

        var sut = new TransactionService(cache.Object, repository.Object, realtime.Object, logger.Object);

        var result = sut.GetAll().ToList();

        result.Should().HaveCount(1);
        result[0].Id.Should().Be("cached-1");
        repository.Verify(r => r.GetRecent(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void GetAll_FallsBackToDatabase_AndBackfillsCache()
    {
        var cache = new Mock<ITransactionStore>();
        var repository = new Mock<ITransactionRepository>();
        var realtime = new Mock<IRealtimePublisher>();
        var logger = new Mock<ILogger<TransactionService>>();

        cache.Setup(c => c.GetAll()).Returns(Array.Empty<Transaction>());

        var dbRows = new[]
        {
            new Transaction { Id = "db-1", Amount = 11m, Currency = "USD", Status = "Pending", Timestamp = DateTime.UtcNow }
        };

        repository.Setup(r => r.GetRecent(It.IsAny<int>())).Returns(dbRows);

        var sut = new TransactionService(cache.Object, repository.Object, realtime.Object, logger.Object);

        var result = sut.GetAll().ToList();

        result.Should().HaveCount(1);
        result[0].Id.Should().Be("db-1");
        cache.Verify(c => c.Upsert(It.Is<Transaction>(t => t.Id == "db-1")), Times.Once);
    }
}
