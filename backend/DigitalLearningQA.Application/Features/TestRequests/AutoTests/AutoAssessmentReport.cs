namespace DigitalLearningQA.Application.Features.AutoTests;

public class AutoAssessmentReport
{
    public int OverallScore { get; set; }
    public string Verdict { get; set; } = string.Empty;
    public string Opinion { get; set; } = string.Empty;
    public bool FileTextExtracted { get; set; }
    public int ExtractedCharacterCount { get; set; }
    public string? ExtractionNotice { get; set; }
    public string AssessmentMethod { get; set; } = "Kiểm tra quy tắc và nội dung tệp";
    public int TechnicalScore { get; set; }
    public int ContentScore { get; set; }
    public int PedagogyScore { get; set; }
    public int ObeScore { get; set; }
    public List<AutoAssessmentFinding> Findings { get; set; } = [];
    public List<string> Recommendations { get; set; } = [];
}

public class AutoAssessmentFinding
{
    public required string Group { get; set; }
    public required string Level { get; set; }
    public required string Problem { get; set; }
    public required string Solution { get; set; }
}
