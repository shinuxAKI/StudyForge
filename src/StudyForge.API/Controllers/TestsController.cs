using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyForge.Domain.Assessment;
using StudyForge.Domain.Questions;
using StudyForge.Infrastructure.Persistence;

namespace StudyForge.API.Controllers;

[ApiController]
[Authorize(Policy = "StudentOrAdmin")]
[Route("api/tests")]
public sealed class TestsController(StudyForgeDbContext db) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost("{questionSetId:guid}/start")]
    public async Task<IActionResult> Start(Guid questionSetId, CancellationToken ct)
    {
        var set = await db.QuestionSets.AsNoTracking().Include(x => x.Questions)
            .SingleOrDefaultAsync(x => x.Id == questionSetId && x.OwnerId == UserId, ct);
        if (set is null) return NotFound();
        if (set.Questions.Count == 0) return BadRequest("This question set has no questions.");
        var attempt = new TestAttempt { QuestionSetId = set.Id, OwnerId = UserId, TotalQuestions = set.Questions.Count };
        db.TestAttempts.Add(attempt); await db.SaveChangesAsync(ct);
        return Ok(new { attempt.Id, attempt.StartedUtc, Questions = set.Questions.OrderBy(x => x.Order)
            .Select(x => new { x.Id, x.Type, x.Prompt, Options = x.Options.OrderBy(o => o.Order).Select(o => new { o.Id, o.Text }) }) });
    }

    [HttpPost("{attemptId:guid}/submit")]
    public async Task<IActionResult> Submit(Guid attemptId, SubmitTestRequest request, CancellationToken ct)
    {
        var attempt = await db.TestAttempts.SingleOrDefaultAsync(x => x.Id == attemptId && x.OwnerId == UserId, ct);
        if (attempt is null) return NotFound();
        if (attempt.Status != TestAttemptStatus.InProgress) return Conflict("This test has already been submitted.");
        var questions = await db.Questions.Where(x => x.QuestionSetId == attempt.QuestionSetId).Include(x => x.Options).ToListAsync(ct);
        if (request.Answers is null || request.Answers.Count != questions.Count ||
            request.Answers.Select(x => x.QuestionId).Distinct().Count() != request.Answers.Count ||
            questions.Any(q => request.Answers.All(a => a.QuestionId != q.Id)))
            return BadRequest("Submit exactly one answer for every question.");
        foreach (var answer in request.Answers)
        {
            var q = questions.Single(x => x.Id == answer.QuestionId);
            var expected = q.Type == QuestionType.MultipleChoice || q.Type == QuestionType.TrueFalse
                ? q.Options.SingleOrDefault(x => x.IsCorrect)?.Text ?? q.CorrectAnswer : q.CorrectAnswer;
            var correct = string.Equals(expected.Trim(), answer.Answer.Trim(), StringComparison.OrdinalIgnoreCase);
            attempt.Answers.Add(new SubmittedAnswer { QuestionId = q.Id, Answer = answer.Answer.Trim(), IsCorrect = correct });
        }
        attempt.TotalQuestions = questions.Count;
        attempt.CorrectAnswers = attempt.Answers.Count(x => x.IsCorrect);
        attempt.ScorePercent = Math.Round((decimal)attempt.CorrectAnswers / attempt.TotalQuestions * 100m, 2);
        attempt.Status = TestAttemptStatus.Submitted; attempt.SubmittedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(new { attempt.Id, attempt.TotalQuestions, attempt.CorrectAnswers, attempt.ScorePercent,
            Review = attempt.Answers.Select(a => new { a.QuestionId, a.Answer, a.IsCorrect }) });
    }

    [HttpGet("{attemptId:guid}/results")]
    public async Task<IActionResult> Results(Guid attemptId, CancellationToken ct)
    {
        var attempt = await db.TestAttempts.AsNoTracking().Include(x => x.Answers)
            .SingleOrDefaultAsync(x => x.Id == attemptId && x.OwnerId == UserId, ct);
        if (attempt is null) return NotFound();
        if (attempt.Status != TestAttemptStatus.Submitted) return Conflict("The test has not been submitted.");
        return Ok(new { attempt.Id, attempt.StartedUtc, attempt.SubmittedUtc, attempt.TotalQuestions, attempt.CorrectAnswers,
            attempt.ScorePercent, Answers = attempt.Answers.Select(x => new { x.QuestionId, x.Answer, x.IsCorrect }) });
    }
}

public sealed record SubmitTestRequest(List<AnswerInput> Answers);
public sealed record AnswerInput(Guid QuestionId, string Answer);