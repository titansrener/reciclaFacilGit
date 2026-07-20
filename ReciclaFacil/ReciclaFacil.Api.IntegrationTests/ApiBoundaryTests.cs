using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ReciclaFacil.Api.IntegrationTests;

public sealed class ApiBoundaryTests :
    IClassFixture<UnavailableDatabaseApiFactory>
{
    private readonly HttpClient _client;

    public ApiBoundaryTests(UnavailableDatabaseApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/api/v1/auth/me")]
    [InlineData("/api/v1/clients/me/overview")]
    [InlineData("/api/v1/cooperatives/me/overview")]
    [InlineData("/api/v1/employees/me/overview")]
    [InlineData("/api/v1/admin/materials")]
    public async Task ProtectedEndpoints_RejectAnonymousRequests(string endpoint)
    {
        var response = await _client.GetAsync(
            endpoint, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_RejectsMissingCredentialsBeforeDatabaseAccess()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "", password = "" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task WebLogin_RequiresWebClientMarker()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/web/login",
            new { email = "usuario@teste.local", password = "qualquer" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
