using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StudyForge.Infrastructure.Persistence;

public sealed class StudyForgeDbContextFactory
    : IDesignTimeDbContextFactory<StudyForgeDbContext>
{
    public StudyForgeDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__StudyForgeDb")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=StudyForgeDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<StudyForgeDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new StudyForgeDbContext(options);
    }
}
