using Microsoft.AspNetCore.Identity;

namespace StudyForge.Infrastructure.Identity;

public sealed class StudyForgeUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
}
