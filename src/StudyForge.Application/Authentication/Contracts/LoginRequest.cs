using System.ComponentModel.DataAnnotations;

namespace StudyForge.Application.Authentication.Contracts;

public sealed class LoginRequest
{
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
