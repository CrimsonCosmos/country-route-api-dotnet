using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CountryRoute.Tests;

public class ApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("/PAN", "PAN", new[] { "USA", "MEX", "GTM", "HND", "NIC", "CRI", "PAN" })]
    [InlineData("/BLZ", "BLZ", new[] { "USA", "MEX", "BLZ" })]
    [InlineData("/api/blz", "BLZ", new[] { "USA", "MEX", "BLZ" })]
    public async Task ValidCode_ReturnsDestinationAndList(string path, string destination, string[] list)
    {
        var response = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(destination, json.GetProperty("destination").GetString());
        Assert.Equal(list, json.GetProperty("list").EnumerateArray().Select(e => e.GetString()));
    }

    [Fact]
    public async Task UnknownCode_Returns404WithJsonError() =>
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/FRA")).StatusCode);

    [Theory]
    [InlineData("/api/PA")]
    [InlineData("/api/P4N")]
    public async Task MalformedCode_Returns400(string path) =>
        Assert.Equal(HttpStatusCode.BadRequest, (await _client.GetAsync(path)).StatusCode);

    [Fact]
    public async Task ApiRoot_ReturnsUsage() =>
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api")).StatusCode);
}
