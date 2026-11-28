namespace StudyForge.Infrastructure.AI;

public sealed class AiGenerationOptions
{
    public const string SectionName = "AI";
    public string Endpoint { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 30;
}