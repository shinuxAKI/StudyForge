using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyForge.Infrastructure.Persistence;

namespace StudyForge.API.Controllers;

[ApiController]
[Authorize(Policy = "StudentOrAdmin")]
[Route("api/flashcards")]
public sealed class FlashcardsController(StudyForgeDbContext db) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetDue([FromQuery] bool dueOnly = true, [FromQuery] int take = 30, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 100);
        var now = DateTime.UtcNow;
        var questions = await (
            from question in db.Questions.AsNoTracking()
            join set in db.QuestionSets.AsNoTracking() on question.QuestionSetId equals set.Id
            where set.OwnerId == UserId
            orderby question.Order
            select new
            {
                question.Id, question.QuestionSetId, SetTitle = set.Title,
                question.Type, question.Prompt, question.CorrectAnswer,
                question.Explanation, question.Difficulty,
                Options = question.Options.OrderBy(option => option.Order)
                    .Select(option => new { option.Id, option.Text }).ToList()
            }).ToListAsync(ct);

        var ids = questions.Select(question => question.Id).ToList();
        var reviews = await db.FlashcardReviews.AsNoTracking()
            .Where(review => review.OwnerId == UserId && ids.Contains(review.QuestionId))
            .ToDictionaryAsync(review => review.QuestionId, ct);

        var cards = questions
            .Where(question => !dueOnly || !reviews.TryGetValue(question.Id, out var review) || review.DueUtc <= now)
            .Take(take)
            .Select(question =>
            {
                reviews.TryGetValue(question.Id, out var review);
                return new
                {
                    question.Id, question.QuestionSetId, question.SetTitle,
                    question.Type, question.Prompt, question.CorrectAnswer,
                    question.Explanation, question.Difficulty, question.Options,
                    DueUtc = review?.DueUtc ?? now,
                    review?.Repetitions,
                    review?.IntervalDays
                };
            }).ToList();

        return Ok(new { TotalDue = cards.Count, Cards = cards });
    }

    [HttpPost("{questionId:guid}/review")]
    public async Task<IActionResult> Review(Guid questionId, ReviewFlashcardRequest request, CancellationToken ct)
    {
        var ownsQuestion = await db.Questions.AnyAsync(question =>
            question.Id == questionId && db.QuestionSets.Any(set =>
                set.Id == question.QuestionSetId && set.OwnerId == UserId), ct);
        if (!ownsQuestion) return NotFound();

        var rating = request.Rating?.Trim().ToLowerInvariant();
        if (rating is not ("again" or "hard" or "good" or "easy"))
            return BadRequest("Rating must be Again, Hard, Good, or Easy.");

        var review = await db.FlashcardReviews.SingleOrDefaultAsync(
            item => item.OwnerId == UserId && item.QuestionId == questionId, ct);
        if (review is null)
        {
            review = new FlashcardReview { OwnerId = UserId, QuestionId = questionId };
            db.FlashcardReviews.Add(review);
        }

        var previousInterval = Math.Max(1, review.IntervalDays);
        switch (rating)
        {
            case "again":
                review.Repetitions = 0;
                review.IntervalDays = 1;
                review.EaseFactor = Math.Max(1.3, review.EaseFactor - 0.2);
                break;
            case "hard":
                review.Repetitions++;
                review.IntervalDays = Math.Max(1, (int)Math.Round(previousInterval * 1.2));
                review.EaseFactor = Math.Max(1.3, review.EaseFactor - 0.15);
                break;
            case "good":
                review.Repetitions++;
                review.IntervalDays = review.Repetitions switch
                {
                    1 => 1,
                    2 => 3,
                    _ => Math.Max(1, (int)Math.Round(previousInterval * review.EaseFactor))
                };
                break;
            case "easy":
                review.Repetitions++;
                review.IntervalDays = Math.Max(4, (int)Math.Round(previousInterval * review.EaseFactor * 1.3));
                review.EaseFactor += 0.15;
                break;
        }

        review.LastReviewedUtc = DateTime.UtcNow;
        review.DueUtc = review.LastReviewedUtc.Value.AddDays(review.IntervalDays);
        await db.SaveChangesAsync(ct);

        return Ok(new
        {
            questionId,
            rating,
            review.Repetitions,
            review.IntervalDays,
            review.EaseFactor,
            review.LastReviewedUtc,
            review.DueUtc
        });
    }
}

public sealed record ReviewFlashcardRequest(string Rating);
