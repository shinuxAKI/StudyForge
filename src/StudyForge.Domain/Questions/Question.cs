namespace StudyForge.Domain.Questions;

public enum QuestionType { MultipleChoice = 1, TrueFalse = 2, FillInBlank = 3, ShortAnswer = 4 }

public sealed class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuestionSetId { get; set; }
    public QuestionSet? QuestionSet { get; set; }
    public QuestionType Type { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Medium";
    public int Order { get; set; }
    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}

public sealed class QuestionOption
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuestionId { get; set; }
    public Question? Question { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Order { get; set; }
}