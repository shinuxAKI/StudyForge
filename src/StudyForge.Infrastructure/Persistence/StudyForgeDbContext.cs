using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyForge.Infrastructure.Identity;

namespace StudyForge.Infrastructure.Persistence;

public sealed class StudyForgeDbContext(
    DbContextOptions<StudyForgeDbContext> options)
    : IdentityDbContext<StudyForgeUser>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<StudyForgeUser>()
            .Property(user => user.DisplayName)
            .HasMaxLength(80)
            .IsRequired();

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(token => token.Id);
            entity.Property(token => token.UserId).IsRequired();
            entity.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.Property(token => token.ReplacedByTokenHash).HasMaxLength(64);

            entity.HasOne(token => token.User)
                .WithMany()
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
