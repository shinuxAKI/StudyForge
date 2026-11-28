using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using StudyForge.Application.AI;
using StudyForge.Domain.Questions;

namespace StudyForge.Infrastructure.AI;

public sealed class AiQuestionGenerationService(HttpClient http, IOptions<AiGenerationOptions> options)
    : IQuestionGenerationService
{
    private readonly AiGenerationOptions _options = options.Value;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<GeneratedQuestion>> GenerateAsync(string material, int count, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint) || string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.Model))
            throw new InvalidOperationException("AI generation is not configured. Use an existing question set as fallback.");
        var schema = new {
            type = "object", additionalProperties = false, required = new[] { "questions" },
            properties = new { questions = new { type = "array", minItems = count, maxItems = count,
                items = new { type = "object", additionalProperties = false,
                    required = new[] { "type", "prompt", "correctAnswer", "explanation", "difficulty", "options" },
                    properties = new {
                        type = new { type = "string", @enum = new[] { "MultipleChoice", "TrueFalse", "FillInBlank", "ShortAnswer" } },
                        prompt = new { type = "string" }, correctAnswer = new { type = "string" },
                        explanation = new { type = "string" },
                        difficulty = new { type = "string", @enum = new[] { "Easy", "Medium", "Hard" } },
                        options = new { type = "array", items = new { type = "object", additionalProperties = false,
                            required = new[] { "text", "isCorrect" },
                            properties = new { text = new { type = "string" }, isCorrect = new { type = "boolean" } } } }
                    }
                }
            } }
        };
        var body = new {
            model = _options.Model,
            messages = new[] {
                new { role = "system", content = "Create study questions only from the supplied source material. Do not use outside facts. Return JSON only matching the supplied schema. For multiple choice use 4 options and exactly one correct option; true/false uses exactly 2 options and exactly one correct option; fill-in and short answer use no options. Explanations must be supported by the source." },
                new { role = "user", content = $"Generate exactly {count} distinct questions from this material:\n\n{material}" }
            },
            response_format = new { type = "json_schema", json_schema = new { name = "studyforge_questions", strict = true, schema } }
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var content = document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(content)) throw new JsonException("AI returned empty content.");
        var envelope = JsonSerializer.Deserialize<Envelope>(content, JsonOptions) ?? throw new JsonException("AI JSON was empty.");
        Validate(envelope.Questions, count);
        return envelope.Questions.Select(q => new GeneratedQuestion(
            Enum.Parse<QuestionType>(q.Type), q.Prompt.Trim(), q.CorrectAnswer.Trim(), q.Explanation.Trim(),
            q.Difficulty, q.Options.Select(o => new GeneratedOption(o.Text.Trim(), o.IsCorrect)).ToList())).ToList();
    }

    private static void Validate(List<QuestionDto> questions, int count)
    {
        if (questions.Count != count) throw new JsonException("AI returned an unexpected question count.");
        var prompts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var q in questions)
        {
            if (!Enum.TryParse<QuestionType>(q.Type, out var type) || string.IsNullOrWhiteSpace(q.Prompt) ||
                q.Prompt.Length > 1000 || string.IsNullOrWhiteSpace(q.CorrectAnswer) ||
                string.IsNullOrWhiteSpace(q.Explanation) || !new[] { "Easy", "Medium", "Hard" }.Contains(q.Difficulty) ||
                !prompts.Add(q.Prompt.Trim())) throw new JsonException("AI output failed question validation.");
            if (type is QuestionType.MultipleChoice or QuestionType.TrueFalse)
            {
                var expected = type == QuestionType.TrueFalse ? 2 : 4;
                if (q.Options.Count != expected || q.Options.Count(o => o.IsCorrect) != 1 ||
                    q.Options.Any(o => string.IsNullOrWhiteSpace(o.Text))) throw new JsonException("AI output has invalid options.");
                if (!q.Options.Any(o => o.IsCorrect && string.Equals(o.Text.Trim(), q.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase)))
                    throw new JsonException("Correct answer does not match the correct option.");
            }
            else if (q.Options.Count != 0) throw new JsonException("Non-choice question unexpectedly contains options.");
        }
    }

    private sealed record Envelope(List<QuestionDto> Questions);
    private sealed record QuestionDto(string Type, string Prompt, string CorrectAnswer, string Explanation, string Difficulty, List<OptionDto> Options);
    private sealed record OptionDto(string Text, bool IsCorrect);
}