using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ReciclaFacil.Infrastructure.Data;

namespace ReciclaFacil.Api.Health;

public sealed class DatabaseHealthCheck(
    ReciclaFacilDbContext database) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await database.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("SQL Server acessível.")
                : HealthCheckResult.Unhealthy("SQL Server inacessível.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Falha ao consultar o SQL Server.", exception);
        }
    }
}
