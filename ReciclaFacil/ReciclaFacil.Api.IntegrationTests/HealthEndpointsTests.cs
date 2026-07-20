using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ReciclaFacil.Infrastructure.Data;
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
    private const string UnavailableConnection =
        "Server=127.0.0.1,1;Database=Indisponivel;" +
        "Integrated Security=True;Encrypt=False;" +
        "TrustServerCertificate=True;Connect Timeout=1";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
            ReplaceDatabase(services, UnavailableConnection));
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] =
                    "integration-tests-only-signing-key-32-bytes"
            });
        });
    }

    internal static void ReplaceDatabase(
        IServiceCollection services,
        string connectionString)
    {
        services.RemoveAll<DbContextOptions<ReciclaFacilDbContext>>();
        services.RemoveAll<ReciclaFacilDbContext>();
        services.AddDbContext<ReciclaFacilDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.UseNetTopologySuite()));
    }
}
