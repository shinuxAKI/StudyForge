namespace StudyForge.Domain.Questions;

public sealed class QuestionSet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = string.Empty;
    public Guid? StudyMaterialId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Source { get; set; } = "Manual";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}