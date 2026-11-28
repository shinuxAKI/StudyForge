using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyForge.Domain.StudyContent;
using StudyForge.Infrastructure.Persistence;

namespace StudyForge.API.Controllers;

[ApiController]
[Authorize(Policy = "StudentOrAdmin")]
[Route("api")]
public sealed class StudyContentController(StudyForgeDbContext db) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("subjects")]
    public async Task<IActionResult> GetSubjects(CancellationToken ct) =>
        Ok(await db.Subjects.AsNoTracking().Where(x => x.OwnerId == UserId)
            .OrderBy(x => x.Name).Select(x => new { x.Id, x.Name, x.Description, x.CreatedUtc })
            .ToListAsync(ct));

    [HttpPost("subjects")]
    public async Task<IActionResult> CreateSubject(SubjectInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name) || input.Name.Trim().Length > 100)
            return BadRequest("Subject name is required and must be at most 100 characters.");
        var item = new Subject { OwnerId = UserId, Name = input.Name.Trim(), Description = Clean(input.Description, 500) };
        db.Subjects.Add(item); await db.SaveChangesAsync(ct);
        return Created($"/api/subjects/{item.Id}", new { item.Id, item.Name, item.Description });
    }

    [HttpPut("subjects/{id:guid}")]
    public async Task<IActionResult> UpdateSubject(Guid id, SubjectInput input, CancellationToken ct)
    {
        var item = await db.Subjects.SingleOrDefaultAsync(x => x.Id == id && x.OwnerId == UserId, ct);
        if (item is null) return NotFound();
        if (string.IsNullOrWhiteSpace(input.Name) || input.Name.Trim().Length > 100) return BadRequest("Invalid subject name.");
        item.Name = input.Name.Trim(); item.Description = Clean(input.Description, 500); item.UpdatedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpDelete("subjects/{id:guid}")]
    public async Task<IActionResult> DeleteSubject(Guid id, CancellationToken ct)
    {
        var item = await db.Subjects.SingleOrDefaultAsync(x => x.Id == id && x.OwnerId == UserId, ct);
        if (item is null) return NotFound();
        db.Subjects.Remove(item); await db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpGet("subjects/{subjectId:guid}/materials")]
    public async Task<IActionResult> GetMaterials(Guid subjectId, CancellationToken ct)
    {
        if (!await db.Subjects.AnyAsync(x => x.Id == subjectId && x.OwnerId == UserId, ct)) return NotFound();
        return Ok(await db.StudyMaterials.AsNoTracking().Where(x => x.SubjectId == subjectId && x.OwnerId == UserId)
            .Select(x => new { x.Id, x.Title, x.Content, x.CreatedUtc, x.UpdatedUtc }).ToListAsync(ct));
    }

    [HttpPost("subjects/{subjectId:guid}/materials")]
    public async Task<IActionResult> CreateMaterial(Guid subjectId, MaterialInput input, CancellationToken ct)
    {
        if (!await db.Subjects.AnyAsync(x => x.Id == subjectId && x.OwnerId == UserId, ct)) return NotFound();
        if (string.IsNullOrWhiteSpace(input.Title) || input.Title.Trim().Length > 160 ||
            string.IsNullOrWhiteSpace(input.Content) || input.Content.Length > 100_000)
            return BadRequest("A title (max 160 characters) and content (max 100,000 characters) are required.");
        var item = new StudyMaterial { SubjectId = subjectId, OwnerId = UserId, Title = input.Title.Trim(), Content = input.Content.Trim() };
        db.StudyMaterials.Add(item); await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetMaterials), new { subjectId }, new { item.Id, item.Title });
    }

    [HttpPut("materials/{id:guid}")]
    public async Task<IActionResult> UpdateMaterial(Guid id, MaterialInput input, CancellationToken ct)
    {
        var item = await db.StudyMaterials.SingleOrDefaultAsync(x => x.Id == id && x.OwnerId == UserId, ct);
        if (item is null) return NotFound();
        if (string.IsNullOrWhiteSpace(input.Title) || input.Title.Trim().Length > 160 ||
            string.IsNullOrWhiteSpace(input.Content) || input.Content.Length > 100_000) return BadRequest("Invalid material.");
        item.Title = input.Title.Trim(); item.Content = input.Content.Trim(); item.UpdatedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpDelete("materials/{id:guid}")]
    public async Task<IActionResult> DeleteMaterial(Guid id, CancellationToken ct)
    {
        var item = await db.StudyMaterials.SingleOrDefaultAsync(x => x.Id == id && x.OwnerId == UserId, ct);
        if (item is null) return NotFound();
        db.StudyMaterials.Remove(item); await db.SaveChangesAsync(ct); return NoContent();
    }

    private static string? Clean(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, max)];
}

public sealed record SubjectInput(string Name, string? Description);
public sealed record MaterialInput(string Title, string Content);