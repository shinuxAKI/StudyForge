using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StudyForge.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminController : ControllerBase
{
    [HttpGet("access")]
    public IActionResult CheckAccess()
    {
        return Ok(new
        {
            message = "Administrator access granted.",
            user = User.Identity?.Name
        });
    }
}
