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
            ?? "Data Source=studyforge.db";

        var options = new DbContextOptionsBuilder<StudyForgeDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new StudyForgeDbContext(options);
    }
}
