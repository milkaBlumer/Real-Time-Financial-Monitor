using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SQLink.Models;
using Xunit;

namespace server_side2.Tests.Integration;

public class IngestApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string Endpoint = "/api/Ingest";

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

        var postResponse = await _client.PostAsJsonAsync(Endpoint, payload);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var getResponse = await _client.GetAsync(Endpoint);
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

        var first = await _client.PostAsJsonAsync(Endpoint, payload);
        first.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var second = await _client.PostAsJsonAsync(Endpoint, payload);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task BurstOfConcurrentUniqueTransactions_AllAccepted_AndPersisted()
    {
        const int count = 150;
        var idPrefix = $"load-unique-{Guid.NewGuid():N}";

        var requests = Enumerable.Range(1, count)
            .Select(index => new Transaction
            {
                Id = $"{idPrefix}-{index}",
                Amount = 10m + index,
                Currency = "USD",
                Status = index % 2 == 0 ? "Completed" : "Pending",
                Timestamp = DateTime.UtcNow
            })
            .ToList();

        var responses = await Task.WhenAll(requests.Select(request => _client.PostAsJsonAsync(Endpoint, request)));

        responses.Should().OnlyContain(response => response.StatusCode == HttpStatusCode.Accepted);

        var getResponse = await _client.GetAsync(Endpoint);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var transactions = await getResponse.Content.ReadFromJsonAsync<List<Transaction>>();
        transactions.Should().NotBeNull();

        var expectedIds = requests.Select(request => request.Id).ToHashSet();
        var actualIds = transactions!
            .Where(tx => tx.Id.StartsWith(idPrefix, StringComparison.Ordinal))
            .Select(tx => tx.Id)
            .ToHashSet();

        actualIds.Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task ConcurrentDuplicateRequests_OnlyOneAccepted_AndTheRestConflict()
    {
        const int count = 40;
        var duplicateId = $"load-dup-{Guid.NewGuid():N}";

        var requests = Enumerable.Range(1, count)
            .Select(_ => new Transaction
            {
                Id = duplicateId,
                Amount = 77.7m,
                Currency = "USD",
                Status = "Pending",
                Timestamp = DateTime.UtcNow
            })
            .ToList();

        var responses = await Task.WhenAll(requests.Select(request => _client.PostAsJsonAsync(Endpoint, request)));

        responses.Count(response => response.StatusCode == HttpStatusCode.Accepted).Should().Be(1);
        responses.Count(response => response.StatusCode == HttpStatusCode.Conflict).Should().Be(count - 1);
    }
}
