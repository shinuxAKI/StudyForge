using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace StudyForge.Web.Services;

public sealed class StudyForgeApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ApiHealthResponse?> GetHealthAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<ApiHealthResponse>("api/health", cancellationToken);

    public Task<ApiAuthResponse?> LoginAsync(string email, string password, CancellationToken ct = default) =>
        SendAuthAsync("api/auth/login", new { email, password }, ct);

    public Task<ApiAuthResponse?> RegisterAsync(string displayName, string email, string password, CancellationToken ct = default) =>
        SendAuthAsync("api/auth/register", new { displayName, email, password }, ct);

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/auth/logout", new { refreshToken }, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<SubjectSummary>> GetSubjectsAsync(string accessToken, CancellationToken ct = default)
    {
        using var request = Authorized(HttpMethod.Get, "api/subjects", accessToken);
        using var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SubjectSummary>>(JsonOptions, ct) ?? [];
    }

    public async Task CreateSubjectAsync(string accessToken, string name, string? description, CancellationToken ct = default)
    {
        using var request = Authorized(HttpMethod.Post, "api/subjects", accessToken);
        request.Content = JsonContent.Create(new { name, description });
        using var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }

    private async Task<ApiAuthResponse?> SendAuthAsync(string path, object body, CancellationToken ct)
    {
        using var response = await httpClient.PostAsJsonAsync(path, body, ct);
        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            var message = "The request could not be completed.";
            try
            {
                using var json = JsonDocument.Parse(payload);
                message = json.RootElement.TryGetProperty("message", out var value)
                    ? value.GetString() ?? message
                    : json.RootElement.TryGetProperty("title", out value)
                        ? value.GetString() ?? message
                        : message;
            }
            catch (JsonException) { }
            throw new ApiRequestException(message);
        }
        return JsonSerializer.Deserialize<ApiAuthResponse>(payload, JsonOptions);
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}

public sealed record ApiHealthResponse(string Status, string Service);
public sealed record ApiAuthResponse(string UserId, string Email, string DisplayName, IReadOnlyList<string> Roles,
    string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, DateTime RefreshTokenExpiresAtUtc);
public sealed record SubjectSummary(Guid Id, string Name, string? Description, DateTime CreatedUtc);
public sealed class ApiRequestException(string message) : Exception(message);
