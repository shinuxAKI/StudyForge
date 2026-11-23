using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyForge.Infrastructure.Persistence;

namespace StudyForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("StudyForgeDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'StudyForgeDb' was not configured.");
        }

        services.AddDbContext<StudyForgeDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}
