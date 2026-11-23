using System.Net.Http.Json;

namespace StudyForge.Web.Services;

public sealed class StudyForgeApiClient(HttpClient httpClient)
{
    public async Task<ApiHealthResponse?> GetHealthAsync(
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<ApiHealthResponse>(
            "api/health",
            cancellationToken);
    }
}

public sealed record ApiHealthResponse(string Status, string Service);
