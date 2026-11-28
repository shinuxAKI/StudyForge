using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyForge.Application.AI;
using StudyForge.Domain.Questions;
using StudyForge.Infrastructure.Persistence;

namespace StudyForge.API.Controllers;

[ApiController]
[Authorize(Policy = "StudentOrAdmin")]
[Route("api/ai/questions")]
public sealed class AiQuestionGenerationController(StudyForgeDbContext db, IQuestionGenerationService generator) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(GenerateQuestionsRequest request, CancellationToken ct)
    {
        if (request.Count is < 1 or > 20) return BadRequest("Question count must be between 1 and 20.");
        var material = await db.StudyMaterials.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.StudyMaterialId && x.OwnerId == UserId, ct);
        if (material is null) return NotFound("Study material not found.");
        try
        {
            var generated = await generator.GenerateAsync(material.Content, request.Count, ct);
            var set = new QuestionSet { OwnerId = UserId, StudyMaterialId = material.Id, Title = request.Title?.Trim() is { Length: > 0 } title ? title[..Math.Min(title.Length, 160)] : $"Practice: {material.Title}", Source = "AI" };
            foreach (var q in generated.Select((q, i) => (q, i)))
                set.Questions.Add(new Question { Type = q.q.Type, Prompt = q.q.Prompt, CorrectAnswer = q.q.CorrectAnswer,
                    Explanation = q.q.Explanation, Difficulty = q.q.Difficulty, Order = q.i,
                    Options = q.q.Options.Select((o, i) => new QuestionOption { Text = o.Text, IsCorrect = o.IsCorrect, Order = i }).ToList() });
            db.QuestionSets.Add(set); await db.SaveChangesAsync(ct);
            return Created($"/api/question-sets/{set.Id}", new { set.Id, set.Title, set.Source, QuestionCount = set.Questions.Count });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or InvalidOperationException or TaskCanceledException)
        {
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "AI generation unavailable",
                detail: "No AI-generated questions were saved. Use a previously saved question set to continue studying, or retry later.");
        }
    }
}

public sealed record GenerateQuestionsRequest(Guid StudyMaterialId, int Count, string? Title);