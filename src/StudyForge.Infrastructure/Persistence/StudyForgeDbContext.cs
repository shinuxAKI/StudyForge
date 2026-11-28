using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyForge.Domain.Assessment;
using StudyForge.Domain.Questions;
using StudyForge.Domain.StudyContent;
using StudyForge.Infrastructure.Identity;
namespace StudyForge.Infrastructure.Persistence;
public sealed class StudyForgeDbContext(DbContextOptions<StudyForgeDbContext> options):IdentityDbContext<StudyForgeUser>(options)
{
 public DbSet<RefreshToken> RefreshTokens=>Set<RefreshToken>(); public DbSet<Subject> Subjects=>Set<Subject>();
 public DbSet<StudyMaterial> StudyMaterials=>Set<StudyMaterial>(); public DbSet<QuestionSet> QuestionSets=>Set<QuestionSet>();
 public DbSet<Question> Questions=>Set<Question>(); public DbSet<QuestionOption> QuestionOptions=>Set<QuestionOption>();
 public DbSet<TestAttempt> TestAttempts=>Set<TestAttempt>(); public DbSet<SubmittedAnswer> SubmittedAnswers=>Set<SubmittedAnswer>();
 protected override void OnModelCreating(ModelBuilder b)
 {
  base.OnModelCreating(b); b.Entity<StudyForgeUser>().Property(x=>x.DisplayName).HasMaxLength(80).IsRequired();
  b.Entity<RefreshToken>(e=>{e.HasKey(x=>x.Id);e.Property(x=>x.UserId).IsRequired();e.Property(x=>x.TokenHash).HasMaxLength(64).IsRequired();e.HasIndex(x=>x.TokenHash).IsUnique();e.Property(x=>x.ReplacedByTokenHash).HasMaxLength(64);e.Property(x=>x.RowVersion).IsRowVersion();e.HasOne(x=>x.User).WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Cascade);});
  b.Entity<Subject>(e=>{e.HasKey(x=>x.Id);e.Property(x=>x.OwnerId).HasMaxLength(450).IsRequired();e.Property(x=>x.Name).HasMaxLength(100).IsRequired();e.Property(x=>x.Description).HasMaxLength(500);e.HasIndex(x=>new{x.OwnerId,x.Name});});
  b.Entity<StudyMaterial>(e=>{e.HasKey(x=>x.Id);e.Property(x=>x.OwnerId).HasMaxLength(450).IsRequired();e.Property(x=>x.Title).HasMaxLength(160).IsRequired();e.Property(x=>x.Content).HasMaxLength(100000).IsRequired();e.HasOne(x=>x.Subject).WithMany(x=>x.Materials).HasForeignKey(x=>x.SubjectId).OnDelete(DeleteBehavior.Cascade);e.HasIndex(x=>new{x.OwnerId,x.SubjectId});});
  b.Entity<QuestionSet>(e=>{e.HasKey(x=>x.Id);e.Property(x=>x.OwnerId).HasMaxLength(450).IsRequired();e.Property(x=>x.Title).HasMaxLength(160).IsRequired();e.Property(x=>x.Source).HasMaxLength(20).IsRequired();e.HasMany(x=>x.Questions).WithOne(x=>x.QuestionSet).HasForeignKey(x=>x.QuestionSetId).OnDelete(DeleteBehavior.Cascade);});
  b.Entity<Question>(e=>{e.HasKey(x=>x.Id);e.Property(x=>x.Prompt).HasMaxLength(1000).IsRequired();e.Property(x=>x.CorrectAnswer).HasMaxLength(1000).IsRequired();e.Property(x=>x.Explanation).HasMaxLength(2000);e.Property(x=>x.Difficulty).HasMaxLength(10).IsRequired();e.HasMany(x=>x.Options).WithOne(x=>x.Question).HasForeignKey(x=>x.QuestionId).OnDelete(DeleteBehavior.Cascade);});
  b.Entity<QuestionOption>(e=>{e.HasKey(x=>x.Id);e.Property(x=>x.Text).HasMaxLength(300).IsRequired();});
  b.Entity<TestAttempt>(e=>{e.HasKey(x=>x.Id);e.Property(x=>x.OwnerId).HasMaxLength(450).IsRequired();e.Property(x=>x.ScorePercent).HasPrecision(5,2);e.HasMany(x=>x.Answers).WithOne(x=>x.TestAttempt).HasForeignKey(x=>x.TestAttemptId).OnDelete(DeleteBehavior.Cascade);e.HasIndex(x=>new{x.OwnerId,x.StartedUtc});});
  b.Entity<SubmittedAnswer>(e=>{e.HasKey(x=>x.Id);e.Property(x=>x.Answer).HasMaxLength(2000).IsRequired();});
 }
}