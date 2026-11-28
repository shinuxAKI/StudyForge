using Microsoft.AspNetCore.Mvc;
using StudyForge.Web.Services;

namespace StudyForge.Web.Controllers;

public sealed class AccountController(StudyForgeApiClient api) : Controller
{
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var result = await api.LoginAsync(model.Email, model.Password, ct);
            if (result is null) throw new ApiRequestException("The API returned an empty response.");
            SaveSession(result);
            return RedirectToAction("Index", "Study");
        }
        catch (ApiRequestException ex) { ModelState.AddModelError(string.Empty, ex.Message); return View(model); }
        catch (HttpRequestException) { ModelState.AddModelError(string.Empty, "StudyForge API is unavailable. Start the API and try again."); return View(model); }
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var result = await api.RegisterAsync(model.DisplayName, model.Email, model.Password, ct);
            if (result is null) throw new ApiRequestException("The API returned an empty response.");
            SaveSession(result);
            return RedirectToAction("Index", "Study");
        }
        catch (ApiRequestException ex) { ModelState.AddModelError(string.Empty, ex.Message); return View(model); }
        catch (HttpRequestException) { ModelState.AddModelError(string.Empty, "StudyForge API is unavailable. Start the API and try again."); return View(model); }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refreshToken = HttpContext.Session.GetString("RefreshToken");
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            try { await api.LogoutAsync(refreshToken, ct); }
            catch (HttpRequestException) { }
        }
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    private void SaveSession(ApiAuthResponse result)
    {
        HttpContext.Session.SetString("AccessToken", result.AccessToken);
        HttpContext.Session.SetString("RefreshToken", result.RefreshToken);
        HttpContext.Session.SetString("DisplayName", result.DisplayName);
        HttpContext.Session.SetString("Email", result.Email);
    }
}

public sealed class LoginViewModel
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required]
    public string Password { get; set; } = "";
}

public sealed class RegisterViewModel
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(80, MinimumLength = 2)]
    public string DisplayName { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = "";
}
