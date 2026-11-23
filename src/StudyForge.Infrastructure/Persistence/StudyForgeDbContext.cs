using Microsoft.EntityFrameworkCore;

namespace StudyForge.Infrastructure.Persistence;

public sealed class StudyForgeDbContext(DbContextOptions<StudyForgeDbContext> options)
    : DbContext(options)
{
    // Add DbSet<TEntity> properties as domain entities are introduced.
}
