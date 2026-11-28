using Microsoft.AspNetCore.Mvc;
using StudyForge.Web.Services;

namespace StudyForge.Web.Controllers;

public sealed class StudyController(StudyForgeApiClient api) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var token = HttpContext.Session.GetString("AccessToken");
        if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "Account");
        ViewBag.DisplayName = HttpContext.Session.GetString("DisplayName") ?? "Student";
        try { return View(await api.GetSubjectsAsync(token, ct)); }
        catch (HttpRequestException) { ViewBag.Error = "Could not reach the StudyForge API."; return View(Array.Empty<SubjectSummary>()); }
        catch (ApiRequestException) { HttpContext.Session.Clear(); return RedirectToAction("Login", "Account"); }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSubject(string name, string? description, CancellationToken ct)
    {
        var token = HttpContext.Session.GetString("AccessToken");
        if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "Account");
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
        {
            TempData["Error"] = "Enter a subject name up to 100 characters.";
            return RedirectToAction(nameof(Index));
        }
        try { await api.CreateSubjectAsync(token, name.Trim(), description, ct); }
        catch (HttpRequestException) { TempData["Error"] = "StudyForge API is unavailable."; }
        catch (ApiRequestException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }
}
