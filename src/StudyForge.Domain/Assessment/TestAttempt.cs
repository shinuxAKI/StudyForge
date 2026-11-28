namespace StudyForge.Domain.Assessment;

public enum TestAttemptStatus { InProgress = 1, Submitted = 2 }

public sealed class TestAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuestionSetId { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public DateTime StartedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedUtc { get; set; }
    public TestAttemptStatus Status { get; set; } = TestAttemptStatus.InProgress;
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public decimal ScorePercent { get; set; }
    public ICollection<SubmittedAnswer> Answers { get; set; } = new List<SubmittedAnswer>();
}

public sealed class SubmittedAnswer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TestAttemptId { get; set; }
    public TestAttempt? TestAttempt { get; set; }
    public Guid QuestionId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}