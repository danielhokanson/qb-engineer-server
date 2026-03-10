using System.Net;

namespace QBEngineer.Tests.Integration;

[Collection(IntegrationTestCollection.Name)]
public class ApiEndpointTests
{
    private readonly HttpClient _client;

    public ApiEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/api/v1/jobs")]
    [InlineData("/api/v1/parts")]
    [InlineData("/api/v1/customers")]
    [InlineData("/api/v1/expenses")]
    [InlineData("/api/v1/leads")]
    [InlineData("/api/v1/assets")]
    [InlineData("/api/v1/inventory/locations")]
    [InlineData("/api/v1/time-tracking/entries")]
    [InlineData("/api/v1/vendors")]
    [InlineData("/api/v1/purchase-orders")]
    [InlineData("/api/v1/orders")]
    [InlineData("/api/v1/quotes")]
    [InlineData("/api/v1/shipments")]
    [InlineData("/api/v1/invoices")]
    public async Task ProtectedEndpoints_WithoutAuth_Return401(string endpoint)
    {
        var response = await _client.GetAsync(endpoint);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/admin/users")]
    [InlineData("/api/v1/admin/reference-data")]
    [InlineData("/api/v1/track-types")]
    [InlineData("/api/v1/reports/jobs-by-stage")]
    public async Task AdminEndpoints_WithoutAuth_Return401(string endpoint)
    {
        var response = await _client.GetAsync(endpoint);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
