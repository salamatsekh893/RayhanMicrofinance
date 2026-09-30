using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RayhanMicrofinance.Application.Interfaces;
using RayhanMicrofinance.Application.Services;
using RayhanMicrofinance.Infrastructure.Data;
using RayhanMicrofinance.Infrastructure.Services;

namespace RayhanMicrofinance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["DatabaseProvider"] ?? "SqlServer";
        var sqlServerConn = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Database=RayhanMicrofinanceDb;Trusted_Connection=True;TrustServerCertificate=True;";
        var sqliteConn = configuration.GetConnectionString("SqliteConnection") 
            ?? "Data Source=rayhan_microfinance.db";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(sqliteConn);
            }
            else
            {
                // Default SQL Server
                options.UseSqlServer(sqlServerConn, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            }
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAccountingService, AccountingService>();
        services.AddScoped<ILoanCalculationService, LoanCalculationService>();

        return services;
    }
}
