using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ReciclaFacil.Api.IntegrationTests;

public sealed class RateLimitingTests :
    IClassFixture<RateLimitingApiFactory>
{
    private readonly HttpClient _client;

    public RateLimitingTests(RateLimitingApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_BlocksEleventhAttemptInOneMinute()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        for (var attempt = 1; attempt <= 10; attempt++)
        {
            var response = await _client.PostAsJsonAsync(
                "/api/v1/auth/login",
                new { email = "", password = "" },
                cancellationToken);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        var blocked = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "", password = "" },
            cancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
    }

    [Fact]
    public async Task Registration_BlocksEleventhAttemptInOneHour()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var invalidRegistration = new
        {
            email = "",
            password = "",
            confirmPassword = "",
            cnpj = "",
            corporateName = "",
            address = "",
            city = "",
            state = ""
        };

        for (var attempt = 1; attempt <= 10; attempt++)
        {
            var response = await _client.PostAsJsonAsync(
                "/api/v1/auth/register/cooperatives",
                invalidRegistration,
                cancellationToken);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        var blocked = await _client.PostAsJsonAsync(
            "/api/v1/auth/register/cooperatives",
            invalidRegistration,
            cancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
    }

    [Fact]
    public async Task PasswordReset_BlocksSixthAttemptInFifteenMinutes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var invalidReset = new
        {
            token = "",
            newPassword = "",
            confirmPassword = ""
        };

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            var response = await _client.PostAsJsonAsync(
                "/api/v1/auth/password/reset",
                invalidReset,
                cancellationToken);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        var blocked = await _client.PostAsJsonAsync(
            "/api/v1/auth/password/reset",
            invalidReset,
            cancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
    }
}

public sealed class RateLimitingApiFactory :
    WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
            UnavailableDatabaseApiFactory.ReplaceDatabase(
                services,
                "Server=127.0.0.1,1;Database=Indisponivel;" +
                "Integrated Security=True;Encrypt=False;" +
                "TrustServerCertificate=True;Connect Timeout=1"));
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] =
                    "integration-tests-only-signing-key-32-bytes"
            });
        });
    }
}
