using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyForge.Domain.Assessment;
using StudyForge.Infrastructure.Persistence;

namespace StudyForge.API.Controllers;

[ApiController]
[Authorize(Policy = "StudentOrAdmin")]
[Route("api/dashboard")]
public sealed class DashboardController(StudyForgeDbContext db) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var subjects = await db.Subjects.CountAsync(item => item.OwnerId == UserId, ct);
        var materials = await db.StudyMaterials.CountAsync(item => item.OwnerId == UserId, ct);
        var questionSets = await db.QuestionSets.Where(item => item.OwnerId == UserId)
            .Select(item => new { item.Id, item.Title, QuestionCount = item.Questions.Count })
            .ToListAsync(ct);
        var attempts = await db.TestAttempts.AsNoTracking()
            .Where(item => item.OwnerId == UserId && item.Status == TestAttemptStatus.Submitted)
            .OrderByDescending(item => item.SubmittedUtc)
            .Select(item => new
            {
                item.Id, item.QuestionSetId, item.StartedUtc, item.SubmittedUtc,
                item.TotalQuestions, item.CorrectAnswers, item.ScorePercent
            }).ToListAsync(ct);
        var now = DateTime.UtcNow;
        var dueFlashcards = await (
            from question in db.Questions
            join set in db.QuestionSets on question.QuestionSetId equals set.Id
            where set.OwnerId == UserId
            join review in db.FlashcardReviews.Where(item => item.OwnerId == UserId)
                on question.Id equals review.QuestionId into reviewGroup
            from review in reviewGroup.DefaultIfEmpty()
            where review == null || review.DueUtc <= now
            select question.Id).CountAsync(ct);

        return Ok(new
        {
            SubjectCount = subjects,
            StudyMaterialCount = materials,
            QuestionSetCount = questionSets.Count,
            QuestionCount = questionSets.Sum(item => item.QuestionCount),
            CompletedTests = attempts.Count,
            AverageScore = attempts.Count == 0 ? 0 : Math.Round(attempts.Average(item => item.ScorePercent), 2),
            DueFlashcards = dueFlashcards,
            RecentTests = attempts.Take(5)
        });
    }
}
