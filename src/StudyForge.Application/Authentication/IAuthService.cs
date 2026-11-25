using StudyForge.Application.Authentication.Contracts;

namespace StudyForge.Application.Authentication;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);

    Task RevokeRefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);
}
