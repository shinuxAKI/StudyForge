using StudyForge.Domain.Questions;

namespace StudyForge.Application.AI;

public sealed record GeneratedQuestion(
    QuestionType Type,
    string Prompt,
    string CorrectAnswer,
    string Explanation,
    string Difficulty,
    IReadOnlyList<GeneratedOption> Options);

public sealed record GeneratedOption(string Text, bool IsCorrect);

public interface IQuestionGenerationService
{
    Task<IReadOnlyList<GeneratedQuestion>> GenerateAsync(string material, int count, CancellationToken cancellationToken);
}