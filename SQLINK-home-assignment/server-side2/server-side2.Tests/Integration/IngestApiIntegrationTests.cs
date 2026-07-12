using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SQLink.Models;
using Xunit;

namespace server_side2.Tests.Integration;

public class IngestApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public IngestApiIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostTransaction_Persists_AndReturnedByGetAll()
    {
        var payload = new Transaction
        {
            Id = "int-1",
            Amount = 99.95m,
            Currency = "USD",
            Status = "Completed",
            Timestamp = DateTime.UtcNow
        };

        var postResponse = await _client.PostAsJsonAsync("/api/Ingest", payload);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var getResponse = await _client.GetAsync("/api/Ingest");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var transactions = await getResponse.Content.ReadFromJsonAsync<List<Transaction>>();
        transactions.Should().NotBeNull();
        transactions!.Should().ContainSingle(t =>
            t.Id == "int-1" &&
            t.Amount == 99.95m &&
            t.Currency == "USD" &&
            t.Status == "Completed");
    }

    [Fact]
    public async Task PostDuplicateId_ReturnsConflict()
    {
        var payload = new Transaction
        {
            Id = "int-dup",
            Amount = 19.5m,
            Currency = "USD",
            Status = "Pending",
            Timestamp = DateTime.UtcNow
        };

        var first = await _client.PostAsJsonAsync("/api/Ingest", payload);
        first.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var second = await _client.PostAsJsonAsync("/api/Ingest", payload);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
