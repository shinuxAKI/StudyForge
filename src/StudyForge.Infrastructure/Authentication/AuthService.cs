using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StudyForge.Application.Authentication;
using StudyForge.Application.Authentication.Contracts;
using StudyForge.Infrastructure.Identity;
using StudyForge.Infrastructure.Persistence;

namespace StudyForge.Infrastructure.Authentication;

public sealed class AuthService(
    UserManager<StudyForgeUser> userManager,
    SignInManager<StudyForgeUser> signInManager,
    StudyForgeDbContext dbContext,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("An account with this email already exists.");
        }

        var user = new StudyForgeUser
        {
            UserName = email,
            Email = email,
            DisplayName = request.DisplayName.Trim()
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(FormatIdentityErrors(createResult));
        }

        var roleResult = await userManager.AddToRoleAsync(user, "Student");
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            throw new InvalidOperationException(FormatIdentityErrors(roleResult));
        }

        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var storedToken = await dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        var now = DateTime.UtcNow;
        if (storedToken is null ||
            storedToken.RevokedAtUtc is not null ||
            storedToken.ExpiresAtUtc <= now)
        {
            throw new UnauthorizedAccessException("The refresh token is invalid or expired.");
        }

        var replacement = GenerateRefreshToken();
        storedToken.RevokedAtUtc = now;
        storedToken.ReplacedByTokenHash = HashToken(replacement.Token);

        var refreshExpiry = now.AddDays(_jwt.RefreshTokenDays);
        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = storedToken.UserId,
            TokenHash = HashToken(replacement.Token),
            CreatedAtUtc = now,
            ExpiresAtUtc = refreshExpiry
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        var roles = await userManager.GetRolesAsync(storedToken.User);
        var access = CreateAccessToken(storedToken.User, roles);

        return new AuthResponse(
            storedToken.User.Id,
            storedToken.User.Email ?? string.Empty,
            storedToken.User.DisplayName,
            roles,
            access.Token,
            access.ExpiresAtUtc,
            replacement.Token,
            refreshExpiry);
    }

    public async Task RevokeRefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var storedToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || storedToken.RevokedAtUtc is not null)
        {
            return;
        }

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponse> CreateSessionAsync(
        StudyForgeUser user,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var refresh = GenerateRefreshToken();
        var refreshExpiry = now.AddDays(_jwt.RefreshTokenDays);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(refresh.Token),
            CreatedAtUtc = now,
            ExpiresAtUtc = refreshExpiry
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        var roles = await userManager.GetRolesAsync(user);
        var access = CreateAccessToken(user, roles);

        return new AuthResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            roles,
            access.Token,
            access.ExpiresAtUtc,
            refresh.Token,
            refreshExpiry);
    }

    private (string Token, DateTime ExpiresAtUtc) CreateAccessToken(StudyForgeUser user, IEnumerable<string> roles)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_jwt.SigningKey);
        var signingKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    private static (string Token, byte[] Bytes) GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return (Convert.ToBase64String(bytes), bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private static string FormatIdentityErrors(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(error => error.Description));
}
