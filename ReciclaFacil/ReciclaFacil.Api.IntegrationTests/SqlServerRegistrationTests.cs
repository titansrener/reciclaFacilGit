using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReciclaFacil.Application.Authentication;
using ReciclaFacil.Application.Registrations;
using Xunit;

namespace ReciclaFacil.Api.IntegrationTests;

public sealed class SqlServerRegistrationTests :
    IClassFixture<SqlServerApiFixture>
{
    private readonly SqlServerApiFixture _fixture;

    public SqlServerRegistrationTests(SqlServerApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CooperativeRegistration_PersistsAccountAndAllowsLogin()
    {
        using var client = _fixture.CreateClient();
        var cancellationToken = TestContext.Current.CancellationToken;
        var command = new RegisterCooperative(
            "cooperativa.integracao@teste.local",
            "Senha123!",
            "Senha123!",
            "12345678000199",
            "Cooperativa de Integração",
            "Rua dos Testes, 100",
            "Fortaleza",
            "CE");

        var registration = await client.PostAsJsonAsync(
            "/api/v1/auth/register/cooperatives",
            command,
            cancellationToken);

        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
        var account = await registration.Content.ReadFromJsonAsync<RegisteredAccount>(
            cancellationToken);
        Assert.NotNull(account);
        Assert.Equal(command.Email, account.Email);
        Assert.Equal("Cooperativa", account.Role);

        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { command.Email, command.Password },
            cancellationToken);

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var tokens = await login.Content.ReadFromJsonAsync<TokenPair>(
            cancellationToken);
        Assert.NotNull(tokens);
        Assert.Equal(account.Id, tokens.User.Id);
        Assert.Equal("Cooperativa", tokens.User.Role);
        Assert.False(string.IsNullOrWhiteSpace(tokens.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens.RefreshToken));

        var duplicate = await client.PostAsJsonAsync(
            "/api/v1/auth/register/cooperatives",
            command with { Cnpj = "12345678000270" },
            cancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal(1, await _fixture.CountAsync(
            "SELECT COUNT(*) FROM dbo.Usuarios WHERE Email = @email",
            new SqlParameter("@email", command.Email),
            cancellationToken));
        Assert.Equal(1, await _fixture.CountAsync(
            "SELECT COUNT(*) FROM dbo.Cooperativas WHERE cooperativaId = @id",
            new SqlParameter("@id", account.Id),
            cancellationToken));
        Assert.Equal(1, await _fixture.CountAsync(
            """
            SELECT COUNT(*)
            FROM dbo.UsuarioRole ur
            INNER JOIN dbo.Roles r ON r.Id = ur.RoleId
            WHERE ur.UserId = @id AND r.Name = N'Cooperativa'
            """,
            new SqlParameter("@id", account.Id),
            cancellationToken));
    }
}

public sealed class SqlServerApiFixture : IAsyncLifetime
{
    private const string ServerInstance = @".\SQLEXPRESS";
    private readonly string _databaseName =
        $"ReciclaFacilWebTests_{Environment.ProcessId}";
    private WebApplicationFactory<Program>? _factory;

    public async ValueTask InitializeAsync()
    {
        await CreateSchemaAsync();
        _factory = new SqlServerWebApplicationFactory(ConnectionString);
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();

        await using var connection = new SqlConnection(MasterConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            IF DB_ID(N'{_databaseName}') IS NOT NULL
            BEGIN
                ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{_databaseName}];
            END
            """;
        await command.ExecuteNonQueryAsync();
    }

    public HttpClient CreateClient() =>
        (_factory ?? throw new InvalidOperationException(
            "O banco de teste ainda não foi inicializado."))
        .CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

    public async Task<int> CountAsync(
        string sql,
        SqlParameter parameter,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(parameter);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    private string MasterConnectionString =>
        new SqlConnectionStringBuilder
        {
            DataSource = ServerInstance,
            InitialCatalog = "master",
            IntegratedSecurity = true,
            Encrypt = false,
            TrustServerCertificate = true,
            ConnectTimeout = 5
        }.ConnectionString;

    private string ConnectionString =>
        new SqlConnectionStringBuilder(MasterConnectionString)
        {
            InitialCatalog = _databaseName,
            MultipleActiveResultSets = true
        }.ConnectionString;

    private async Task CreateSchemaAsync()
    {
        var databaseDirectory = FindDatabaseDirectory();
        var scripts = Directory.GetFiles(databaseDirectory, "*.sql")
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(9, scripts.Length);

        await using var connection = new SqlConnection(MasterConnectionString);
        await connection.OpenAsync();

        foreach (var script in scripts)
        {
            var source = await File.ReadAllTextAsync(script);
            source = source.Replace(
                "ReciclaFacilWeb", _databaseName, StringComparison.Ordinal);

            foreach (var batch in Regex.Split(
                source, @"^\s*GO\s*(?:--.*)?$", RegexOptions.Multiline |
                                                  RegexOptions.IgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(batch))
                    continue;

                await using var command = connection.CreateCommand();
                command.CommandText = batch;
                command.CommandTimeout = 30;
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    private static string FindDatabaseDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "Database");
            if (Directory.Exists(candidate))
                return candidate;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "A pasta Database não foi encontrada a partir do diretório de testes.");
    }

    private sealed class SqlServerWebApplicationFactory(string connectionString) :
        WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureServices(services =>
                UnavailableDatabaseApiFactory.ReplaceDatabase(
                    services, connectionString));
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Jwt:SigningKey"] =
                            "integration-tests-only-signing-key-32-bytes"
                    });
            });
        }
    }
}
