using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyForge.Domain.Assessment;
using StudyForge.Domain.Questions;
using StudyForge.Domain.StudyContent;
using StudyForge.Infrastructure.Identity;

namespace StudyForge.Infrastructure.Persistence;

public sealed class StudyForgeDbContext(
    DbContextOptions<StudyForgeDbContext> options)
    : IdentityDbContext<StudyForgeUser>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<StudyMaterial> StudyMaterials => Set<StudyMaterial>();
    public DbSet<QuestionSet> QuestionSets => Set<QuestionSet>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<TestAttempt> TestAttempts => Set<TestAttempt>();
    public DbSet<SubmittedAnswer> SubmittedAnswers => Set<SubmittedAnswer>();

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
            entity.Property(token => token.RowVersion).IsConcurrencyToken();
            entity.HasOne(token => token.User)
                .WithMany()
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Subject>(entity =>
        {
            entity.HasKey(subject => subject.Id);
            entity.Property(subject => subject.OwnerId).HasMaxLength(450).IsRequired();
            entity.Property(subject => subject.Name).HasMaxLength(100).IsRequired();
            entity.Property(subject => subject.Description).HasMaxLength(500);
            entity.HasIndex(subject => new { subject.OwnerId, subject.Name });
        });

        builder.Entity<StudyMaterial>(entity =>
        {
            entity.HasKey(material => material.Id);
            entity.Property(material => material.OwnerId).HasMaxLength(450).IsRequired();
            entity.Property(material => material.Title).HasMaxLength(160).IsRequired();
            entity.Property(material => material.Content).HasMaxLength(100000).IsRequired();
            entity.HasOne(material => material.Subject)
                .WithMany(subject => subject.Materials)
                .HasForeignKey(material => material.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(material => new { material.OwnerId, material.SubjectId });
        });

        builder.Entity<QuestionSet>(entity =>
        {
            entity.HasKey(set => set.Id);
            entity.Property(set => set.OwnerId).HasMaxLength(450).IsRequired();
            entity.Property(set => set.Title).HasMaxLength(160).IsRequired();
            entity.Property(set => set.Source).HasMaxLength(20).IsRequired();
            entity.HasMany(set => set.Questions)
                .WithOne(question => question.QuestionSet)
                .HasForeignKey(question => question.QuestionSetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Question>(entity =>
        {
            entity.HasKey(question => question.Id);
            entity.Property(question => question.Prompt).HasMaxLength(1000).IsRequired();
            entity.Property(question => question.CorrectAnswer).HasMaxLength(1000).IsRequired();
            entity.Property(question => question.Explanation).HasMaxLength(2000);
            entity.Property(question => question.Difficulty).HasMaxLength(10).IsRequired();
            entity.HasMany(question => question.Options)
                .WithOne(option => option.Question)
                .HasForeignKey(option => option.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(option => option.Id);
            entity.Property(option => option.Text).HasMaxLength(300).IsRequired();
        });

        builder.Entity<TestAttempt>(entity =>
        {
            entity.HasKey(attempt => attempt.Id);
            entity.Property(attempt => attempt.OwnerId).HasMaxLength(450).IsRequired();
            entity.Property(attempt => attempt.ScorePercent).HasPrecision(5, 2);
            entity.HasMany(attempt => attempt.Answers)
                .WithOne(answer => answer.TestAttempt)
                .HasForeignKey(answer => answer.TestAttemptId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(attempt => new { attempt.OwnerId, attempt.StartedUtc });
        });

        builder.Entity<SubmittedAnswer>(entity =>
        {
            entity.HasKey(answer => answer.Id);
            entity.Property(answer => answer.Answer).HasMaxLength(2000).IsRequired();
        });
    }
}
