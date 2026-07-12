using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SQLink.Abstractions;
using SQLink.Controllers;
using SQLink.Models;
using SQLink.Services;
using Xunit;

namespace server_side2.Tests;

public class IngestControllerTests
{
    [Fact]
    public async Task Post_WhenValid_ReturnsAccepted()
    {
        var service = new Mock<ITransactionService>();
        var logger = new Mock<ILogger<IngestController>>();
        var tx = new Transaction { Id = "tx-1", Amount = 10m, Currency = "USD", Status = "Pending", Timestamp = DateTime.UtcNow };

        service.Setup(s => s.IngestAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tx);

        var controller = new IngestController(service.Object, logger.Object);

        var result = await controller.Post(tx);

        result.Should().BeOfType<AcceptedResult>();
    }

    [Fact]
    public async Task Post_WhenConflict_ReturnsConflict()
    {
        var service = new Mock<ITransactionService>();
        var logger = new Mock<ILogger<IngestController>>();
        var tx = new Transaction { Id = "tx-1", Amount = 10m, Currency = "USD", Status = "Pending", Timestamp = DateTime.UtcNow };

        service.Setup(s => s.IngestAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TransactionConflictException("tx-1"));

        var controller = new IngestController(service.Object, logger.Object);

        var result = await controller.Post(tx);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Post_WhenRealtimePublishFails_Returns503()
    {
        var service = new Mock<ITransactionService>();
        var logger = new Mock<ILogger<IngestController>>();
        var tx = new Transaction { Id = "tx-1", Amount = 10m, Currency = "USD", Status = "Pending", Timestamp = DateTime.UtcNow };

        service.Setup(s => s.IngestAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RealtimePublishException("tx-1"));

        var controller = new IngestController(service.Object, logger.Object);

        var result = await controller.Post(tx);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    [Fact]
    public void GetAll_ReturnsOkWithTransactions()
    {
        var service = new Mock<ITransactionService>();
        var logger = new Mock<ILogger<IngestController>>();

        service.Setup(s => s.GetAll()).Returns(new[]
        {
            new Transaction { Id = "tx-1", Amount = 10m, Currency = "USD", Status = "Pending", Timestamp = DateTime.UtcNow }
        });

        var controller = new IngestController(service.Object, logger.Object);

        var result = controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }
}
