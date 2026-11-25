using System.ComponentModel.DataAnnotations;

namespace StudyForge.Application.Authentication.Contracts;

public sealed class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}
