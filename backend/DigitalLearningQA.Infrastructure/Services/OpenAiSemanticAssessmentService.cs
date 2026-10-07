using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Features.AutoTests;
using Microsoft.Extensions.Configuration;

namespace DigitalLearningQA.Infrastructure.Services;

/// <summary>
/// Optional OpenAI integration. No request is made unless OPENAI_API_KEY (or OpenAI:ApiKey)
/// is configured on the server. This keeps credentials out of source control.
/// </summary>
public sealed class OpenAiSemanticAssessmentService : ISemanticAssessmentService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string _model;

    public OpenAiSemanticAssessmentService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? configuration["OpenAI:ApiKey"];
        _model = configuration["OpenAI:Model"] ?? "gpt-5-mini";
    }

    public async Task<SemanticAssessmentResult?> AssessAsync(string materialType, string title, string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(content)) return null;

        // Limit input size while retaining the original submitted text; no invented context is supplied.
        var evidence = content.Length > 45000 ? content[..45000] : content;
        var prompt = $"""
Bạn là chuyên gia kiểm định học liệu số đại học theo OBE. Chỉ đánh giá dựa trên bằng chứng có trong nội dung tệp bên dưới; không suy đoán hoặc bịa thông tin.
Học liệu: {title}
Loại học liệu: {materialType}

Chấm ba nhóm theo thang 0-100: contentScore (cấu trúc/nội dung), pedagogyScore (hoạt động/câu hỏi/hướng dẫn), obeScore (CLO, đánh giá, rubric/ma trận).
Mỗi lỗi phải có group là "Nội dung", "Sư phạm" hoặc "OBE"; level là "Thấp", "Trung bình" hoặc "Cao"; problem nêu thiếu sót có bằng chứng; solution là cách khắc phục cụ thể.
Trả về duy nhất JSON có các trường contentScore, pedagogyScore, obeScore, opinion, findings và recommendations. Mỗi phần tử findings có group, level, problem, solution.

NỘI DUNG TỆP:
{evidence}
""";
        var payload = JsonSerializer.Serialize(new
        {
            model = _model,
            input = prompt,
            text = new { format = new { type = "json_object" } }
        });
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        if (!document.RootElement.TryGetProperty("output_text", out var output) || string.IsNullOrWhiteSpace(output.GetString())) return null;
        var result = JsonSerializer.Deserialize<SemanticAssessmentResult>(output.GetString()!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result is null ? null : new SemanticAssessmentResult
        {
            ContentScore = Math.Clamp(result.ContentScore, 0, 100),
            PedagogyScore = Math.Clamp(result.PedagogyScore, 0, 100),
            ObeScore = Math.Clamp(result.ObeScore, 0, 100),
            Opinion = result.Opinion ?? string.Empty,
            Findings = result.Findings ?? [],
            Recommendations = result.Recommendations ?? []
        };
    }
}
