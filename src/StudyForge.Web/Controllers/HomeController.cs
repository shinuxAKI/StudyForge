using Microsoft.AspNetCore.Mvc;

namespace StudyForge.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
}
