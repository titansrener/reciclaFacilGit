using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReciclaFacil.Application.Cooperatives;
using ReciclaFacil.Application.Materials;
using ReciclaFacil.Application.Authentication;
using ReciclaFacil.Application.Clients;
using ReciclaFacil.Application.Employees;
using ReciclaFacil.Application.Registrations;
using ReciclaFacil.Application.Companies;
using ReciclaFacil.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ReciclaFacil.Infrastructure.Data;
using ReciclaFacil.Infrastructure.Queries;
using ReciclaFacil.Infrastructure.Services;

namespace ReciclaFacil.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReciclaFacilInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ReciclaFacil")
            ?? throw new InvalidOperationException("A conexão ReciclaFacil não foi configurada.");
        services.AddDbContext<ReciclaFacilDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.UseNetTopologySuite()));
        services.AddScoped<ICooperativeQueries, CooperativeQueries>();
        services.AddScoped<IMaterialQueries, MaterialQueries>();
        services.AddScoped<IClientQueries, ClientQueries>();
        services.AddScoped<IClientCollectionService, ClientCollectionService>();
        services.AddScoped<ICooperativeOperations, CooperativeOperations>();
        services.AddScoped<ICooperativeResourceService, CooperativeResourceService>();
        services.AddScoped<IEmployeeOperations, EmployeeOperations>();
        services.AddScoped<IClientFinancialService, ClientFinancialService>();
        services.AddScoped<IMaterialManagementService, MaterialManagementService>();
        services.AddScoped<IPublicRegistrationService, PublicRegistrationService>();
        services.AddScoped<ICompanyQueries, CompanyQueries>();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IPasswordHasher<LegacyUser>>(
            new PasswordHasher<LegacyUser>(Options.Create(new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2
            })));
        services.AddScoped<IApiAuthenticationService, ApiAuthenticationService>();
        return services;
    }
}
