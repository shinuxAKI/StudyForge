namespace StudyForge.Infrastructure.Persistence;

public sealed class FlashcardReview
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = string.Empty;
    public Guid QuestionId { get; set; }
    public int Repetitions { get; set; }
    public int IntervalDays { get; set; }
    public double EaseFactor { get; set; } = 2.5;
    public DateTime? LastReviewedUtc { get; set; }
    public DateTime DueUtc { get; set; } = DateTime.UtcNow;
}
