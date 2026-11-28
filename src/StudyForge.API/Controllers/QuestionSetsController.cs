using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyForge.Domain.Questions;
using StudyForge.Infrastructure.Persistence;

namespace StudyForge.API.Controllers;

[ApiController]
[Authorize(Policy = "StudentOrAdmin")]
[Route("api/question-sets")]
public sealed class QuestionSetsController(StudyForgeDbContext db) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(await db.QuestionSets.AsNoTracking().Where(x => x.OwnerId == UserId)
            .Select(x => new { x.Id, x.Title, x.Source, QuestionCount = x.Questions.Count, x.CreatedUtc }).ToListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var set = await db.QuestionSets.AsNoTracking().Include(x => x.Questions).ThenInclude(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == id && x.OwnerId == UserId, ct);
        return set is null ? NotFound() : Ok(set);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateQuestionSetRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 160) return BadRequest("Title is required (max 160 characters).");
        if (request.Questions is null || request.Questions.Count is < 1 or > 100) return BadRequest("Provide between 1 and 100 questions.");
        if (request.StudyMaterialId.HasValue && !await db.StudyMaterials.AnyAsync(x => x.Id == request.StudyMaterialId && x.OwnerId == UserId, ct))
            return NotFound("Study material not found.");
        var errors = Validate(request.Questions);
        if (errors.Count > 0) return BadRequest(new { errors });
        var set = new QuestionSet { OwnerId = UserId, Title = request.Title.Trim(), StudyMaterialId = request.StudyMaterialId, Source = "Manual" };
        foreach (var q in request.Questions)
            set.Questions.Add(new Question { Type = q.Type, Prompt = q.Prompt.Trim(), CorrectAnswer = q.CorrectAnswer.Trim(),
                Explanation = q.Explanation?.Trim() ?? "", Difficulty = q.Difficulty, Order = q.Order,
                Options = q.Options.Select((o, i) => new QuestionOption { Text = o.Text.Trim(), IsCorrect = o.IsCorrect, Order = i }).ToList() });
        db.QuestionSets.Add(set); await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = set.Id }, new { set.Id, set.Title, QuestionCount = set.Questions.Count });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var set = await db.QuestionSets.SingleOrDefaultAsync(x => x.Id == id && x.OwnerId == UserId, ct);
        if (set is null) return NotFound();
        db.QuestionSets.Remove(set); await db.SaveChangesAsync(ct); return NoContent();
    }

    private static List<string> Validate(IReadOnlyList<QuestionInput> questions)
    {
        var errors = new List<string>();
        for (var i = 0; i < questions.Count; i++)
        {
            var q = questions[i];
            if (string.IsNullOrWhiteSpace(q.Prompt) || q.Prompt.Length > 1000) errors.Add($"Question {i + 1}: prompt is required (max 1000 characters).");
            if (string.IsNullOrWhiteSpace(q.CorrectAnswer)) errors.Add($"Question {i + 1}: correct answer is required.");
            if (!new[] { "Easy", "Medium", "Hard" }.Contains(q.Difficulty)) errors.Add($"Question {i + 1}: invalid difficulty.");
            if (q.Type == QuestionType.MultipleChoice && (q.Options.Count < 2 || q.Options.Count > 6 || q.Options.Count(x => x.IsCorrect) != 1))
                errors.Add($"Question {i + 1}: multiple choice needs 2–6 options and exactly one correct option.");
            if (q.Type == QuestionType.TrueFalse && (q.Options.Count != 2 || q.Options.Count(x => x.IsCorrect) != 1))
                errors.Add($"Question {i + 1}: true/false needs two options and exactly one correct option.");
            if (q.Type is QuestionType.FillInBlank or QuestionType.ShortAnswer && q.Options.Count != 0)
                errors.Add($"Question {i + 1}: this question type must not include options.");
            if (q.Options.Any(x => string.IsNullOrWhiteSpace(x.Text) || x.Text.Length > 300))
                errors.Add($"Question {i + 1}: option text is required (max 300 characters).");
        }
        return errors;
    }
}

public sealed record CreateQuestionSetRequest(string Title, Guid? StudyMaterialId, List<QuestionInput> Questions);
public sealed record QuestionInput(QuestionType Type, string Prompt, string CorrectAnswer, string? Explanation, string Difficulty, int Order, List<OptionInput> Options);
public sealed record OptionInput(string Text, bool IsCorrect);