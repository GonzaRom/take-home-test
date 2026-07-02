using System;
using Fundo.Application.Loans;
using Fundo.Infrastructure.Configuration;
using Fundo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fundo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringNames.Default)
            ?? throw new InvalidOperationException($"{ConnectionStringNames.Default} connection string is required.");

        services.AddDbContext<LoanDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<ILoanRepository, LoanRepository>();

        return services;
    }
}
