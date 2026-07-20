using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ReciclaFacil.Api.IntegrationTests;

public sealed class HealthEndpointsTests :
    IClassFixture<UnavailableDatabaseApiFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointsTests(UnavailableDatabaseApiFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Live_DoesNotDependOnDatabase()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var response = await _client.GetAsync("/health/live", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy",
            await response.Content.ReadAsStringAsync(cancellationToken));
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/ready")]
    public async Task Readiness_IsUnavailableWhenDatabaseCannotBeReached(
        string endpoint)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var response = await _client.GetAsync(endpoint, cancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("Unhealthy",
            await response.Content.ReadAsStringAsync(cancellationToken));
    }
}

public sealed class UnavailableDatabaseApiFactory :
    WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:ReciclaFacil"] =
                    "Server=127.0.0.1,1;Database=Indisponivel;" +
                    "Integrated Security=True;Encrypt=False;" +
                    "TrustServerCertificate=True;Connect Timeout=1",
                ["Jwt:SigningKey"] =
                    "integration-tests-only-signing-key-32-bytes"
            });
        });
    }
}
