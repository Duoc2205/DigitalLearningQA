using DigitalLearningQA.Application.Common.Exceptions;
using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Domain.Entities;
using DigitalLearningQA.Domain.Enums;
using DigitalLearningQA.Application.Features.AutoTests;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DigitalLearningQA.Application.Features.AutoTests.Commands.RunAutoTest;

public class RunAutoTestHandler
    : IRequestHandler<RunAutoTestCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IContentExtractionService _contentExtractor;
    private readonly ISemanticAssessmentService _semanticAssessment;

    public RunAutoTestHandler(
        IApplicationDbContext context,
        IContentExtractionService contentExtractor,
        ISemanticAssessmentService semanticAssessment)
    {
        _context = context;
        _contentExtractor = contentExtractor;
        _semanticAssessment = semanticAssessment;
    }

    public async Task<Guid> Handle(
        RunAutoTestCommand request,
        CancellationToken cancellationToken)
    {
        var version = await _context.MaterialVersions
            .Include(x => x.LearningMaterial)
            .SingleOrDefaultAsync(x => x.Id == request.MaterialVersionId, cancellationToken);

        if (version == null)
            throw new ResourceNotFoundException(
                "Material version not found.");

        var extracted = await _contentExtractor.ExtractAsync(
            version.FilePath,
            Path.GetExtension(version.FileName),
            cancellationToken);
        var report = BuildReport(version, extracted);
        var semanticResult = await _semanticAssessment.AssessAsync(
            version.LearningMaterial.MaterialType.ToString(),
            version.LearningMaterial.Title,
            extracted.Text,
            cancellationToken);
        if (semanticResult is not null)
        {
            report.ContentScore = semanticResult.ContentScore;
            report.PedagogyScore = semanticResult.PedagogyScore;
            report.ObeScore = semanticResult.ObeScore;
            report.Findings.RemoveAll(x => x.Group is "Nội dung" or "Sư phạm" or "OBE");
            report.Findings.AddRange(semanticResult.Findings);
            report.Recommendations = semanticResult.Recommendations;
            report.Opinion = semanticResult.Opinion;
            report.AssessmentMethod = "AI ngữ nghĩa đánh giá nội dung tệp";
            FinalizeReport(report);
        }
        var testRequest = new TestRequest
        {
            Id = Guid.NewGuid(),

            MaterialVersionId = request.MaterialVersionId,

            TesterId = null,

            RequestedDate = DateTime.UtcNow,

            StartedDate = DateTime.UtcNow,

            CompletedDate = DateTime.UtcNow,

            Status = TestRequestStatus.Completed,

            Conclusion = "[AUTO_ASSESSMENT]" + JsonSerializer.Serialize(report)
        };

        // Chỉ học liệu đạt ngưỡng tự động mới vào hàng chờ CCD. Học liệu chưa đạt
        // quay lại cho người nộp sửa và tạo phiên bản mới để AI chấm lại.
        var passedAutoAssessment = report.OverallScore >= 80 && !report.Findings.Any(x => x.Level == "Cao");
        version.Status = passedAutoAssessment ? MaterialStatus.AICompleted : MaterialStatus.NeedRevision;
        version.SubmittedAt = DateTime.UtcNow;
        version.LearningMaterial.Status = passedAutoAssessment ? MaterialStatus.PendingApproval : MaterialStatus.NeedRevision;
        version.LearningMaterial.SubmittedAt = DateTime.UtcNow;
        _context.Notifications.Add(new Notification
        {
            UserId = version.LearningMaterial.OwnerId,
            Type = passedAutoAssessment ? "AiPassed" : "AiRevisionRequired",
            Title = passedAutoAssessment ? "AI đã đánh giá: học liệu đạt" : "AI yêu cầu chỉnh sửa học liệu",
            Message = passedAutoAssessment
                ? $"{version.LearningMaterial.Title} đạt {report.OverallScore}/100. Bạn có thể xuất báo cáo và xác nhận nghiệm thu."
                : $"{version.LearningMaterial.Title} đạt {report.OverallScore}/100 và cần được chỉnh sửa theo báo cáo AI.",
            Link = "materials"
        });
        _context.TestRequests.Add(testRequest);

        await _context.SaveChangesAsync(cancellationToken);

        return testRequest.Id;
    }

    private static AutoAssessmentReport BuildReport(MaterialVersion version, ExtractedContent extracted)
    {
        var material = version.LearningMaterial;
        var text = $"{material.Title} {material.Description} {version.Remark} {extracted.Text}".ToLowerInvariant();
        var report = new AutoAssessmentReport
        {
            FileTextExtracted = extracted.IsAvailable,
            ExtractedCharacterCount = extracted.Text.Length,
            ExtractionNotice = extracted.Notice,
            AssessmentMethod = "Chấm điểm tự động có trọng số theo bằng chứng trong tệp"
        };
        void Issue(string group, string level, string problem, string solution)
            => report.Findings.Add(new AutoAssessmentFinding { Group = group, Level = level, Problem = problem, Solution = solution });
        bool HasAny(params string[] phrases) => phrases.Any(text.Contains);
        var allowed = material.MaterialType switch
        {
            MaterialType.Pdf => new[] { ".pdf" }, MaterialType.Word => new[] { ".doc", ".docx" }, MaterialType.PowerPoint => new[] { ".ppt", ".pptx" },
            MaterialType.Video => new[] { ".mp4", ".webm", ".mov" }, MaterialType.Scorm => new[] { ".zip" }, MaterialType.Html => new[] { ".html", ".htm" }, _ => new[] { ".zip" }
        };
        // Kỹ thuật: 100 điểm chỉ khi đủ bốn bằng chứng, không còn cơ chế mặc định 100.
        if (allowed.Contains(Path.GetExtension(version.FileName).ToLowerInvariant())) report.TechnicalScore += 35;
        else Issue("Kỹ thuật", "Cao", $"Định dạng tệp {Path.GetExtension(version.FileName)} chưa khớp loại học liệu đã chọn.", "Chọn đúng loại học liệu hoặc tải lại tệp theo định dạng phù hợp.");
        if (version.FileSize >= 1024) report.TechnicalScore += 25;
        else Issue("Kỹ thuật", "Cao", "Dung lượng tệp quá nhỏ, cần kiểm tra lại nội dung tệp.", "Mở tệp, xác nhận có đầy đủ nội dung rồi tải lại phiên bản hoàn chỉnh.");
        if (!string.IsNullOrWhiteSpace(version.MimeType)) report.TechnicalScore += 20;
        else Issue("Kỹ thuật", "Trung bình", "Chưa xác định được kiểu MIME của tệp.", "Tải lại tệp từ máy tính với định dạng chuẩn để hệ thống nhận diện chính xác.");
        if (extracted.IsAvailable || !string.Equals(Path.GetExtension(version.FileName), ".pdf", StringComparison.OrdinalIgnoreCase)) report.TechnicalScore += 20;
        else Issue("Kỹ thuật", "Trung bình", "Không đọc được lớp văn bản của PDF để đối chiếu nội dung.", "Dùng PDF có lớp chữ hoặc OCR bản quét trước khi nộp lại.");

        // Nội dung: cấu trúc, độ bao phủ, mục tiêu và nguồn tham khảo.
        if (extracted.IsAvailable || (material.Description?.Trim().Length ?? 0) >= 80) report.ContentScore += 15;
        else Issue("Nội dung", "Trung bình", "Chưa có đủ văn bản để kiểm tra nội dung học liệu.", "Nộp PDF có thể trích xuất chữ hoặc bổ sung mô tả nội dung đầy đủ.");
        if (HasAny("chủ đề", "nội dung", "mục ", "chương", "bài ")) report.ContentScore += 25;
        else Issue("Nội dung", "Trung bình", "Chưa thấy cấu trúc chủ đề hoặc các phần nội dung.", "Bổ sung đề mục, chủ đề và trình tự nội dung trong học liệu.");
        if (HasAny("mục tiêu", "clo", "chuẩn đầu ra")) report.ContentScore += 25;
        else Issue("Nội dung", "Trung bình", "Chưa thấy mục tiêu học tập hoặc chuẩn đầu ra.", "Bổ sung mục tiêu đo lường được và CLO liên quan.");
        if (extracted.Text.Length >= 1000) report.ContentScore += 20;
        else Issue("Nội dung", "Thấp", "Khối lượng văn bản trích xuất còn ít để chứng minh độ bao phủ nội dung.", "Kiểm tra lại tệp và bổ sung nội dung cốt lõi của học liệu.");
        if (HasAny("tài liệu tham khảo", "tham khảo", "nguồn")) report.ContentScore += 15;
        else Issue("Nội dung", "Thấp", "Chưa thấy tài liệu tham khảo hoặc nguồn nội dung.", "Bổ sung nguồn/tài liệu tham khảo phù hợp.");

        // Sư phạm: mỗi bằng chứng có trọng số riêng.
        if (HasAny("hoạt động", "thảo luận", "thực hành")) report.PedagogyScore += 30;
        else Issue("Sư phạm", "Trung bình", "Chưa thấy hoạt động học tập hoặc thực hành.", "Bổ sung hoạt động, bài tập hoặc nhiệm vụ học tập.");
        if (HasAny("câu hỏi", "quiz", "bài tập")) report.PedagogyScore += 25;
        else Issue("Sư phạm", "Trung bình", "Chưa thấy câu hỏi hoặc bài tập đánh giá người học.", "Bổ sung câu hỏi, quiz hoặc bài tập phù hợp.");
        if (HasAny("đáp án", "lời giải", "phản hồi")) report.PedagogyScore += 25;
        else Issue("Sư phạm", "Thấp", "Chưa thấy đáp án, lời giải hoặc phản hồi học tập.", "Bổ sung đáp án/hướng dẫn chấm hoặc phản hồi cho người học.");
        if (HasAny("hướng dẫn", "yêu cầu", "cách thực hiện")) report.PedagogyScore += 20;
        else Issue("Sư phạm", "Thấp", "Chưa thấy hướng dẫn thực hiện cho người học.", "Bổ sung hướng dẫn, yêu cầu đầu ra và cách hoàn thành hoạt động.");

        // OBE: CLO, mapping, rubric và công cụ đánh giá phải có bằng chứng độc lập.
        if (HasAny("clo", "chuẩn đầu ra học phần")) report.ObeScore += 35;
        else Issue("OBE", "Cao", "Chưa thấy CLO hoặc chuẩn đầu ra học phần.", "Bổ sung CLO liên quan đến học liệu.");
        if (HasAny("ma trận", "mapping", "liên kết clo")) report.ObeScore += 30;
        else Issue("OBE", "Trung bình", "Chưa thấy ma trận hoặc mapping giữa hoạt động/đánh giá và CLO.", "Bổ sung ma trận OBE thể hiện liên kết CLO với nội dung và đánh giá.");
        if (HasAny("rubric", "tiêu chí", "thang điểm")) report.ObeScore += 20;
        else Issue("OBE", "Trung bình", "Chưa thấy rubric, tiêu chí hoặc thang điểm đánh giá.", "Bổ sung rubric/tiêu chí đánh giá rõ ràng.");
        if (HasAny("đánh giá", "kiểm tra", "bài thi")) report.ObeScore += 15;
        else Issue("OBE", "Thấp", "Chưa thấy công cụ đánh giá gắn với chuẩn đầu ra.", "Bổ sung câu hỏi hoặc công cụ đánh giá có liên kết CLO.");
        report.TechnicalScore = Math.Max(0, report.TechnicalScore); report.ContentScore = Math.Max(0, report.ContentScore); report.PedagogyScore = Math.Max(0, report.PedagogyScore); report.ObeScore = Math.Max(0, report.ObeScore);
        report.OverallScore = (int)Math.Round(report.TechnicalScore * .25 + report.ContentScore * .25 + report.PedagogyScore * .25 + report.ObeScore * .25);
        report.Recommendations = report.Findings.Select(x => x.Solution).Distinct().ToList();
        report.Recommendations.Insert(0, extracted.IsAvailable
            ? "Hệ thống đã trích xuất và đối chiếu nội dung văn bản trực tiếp từ tệp học liệu đã nộp."
            : extracted.Notice ?? "Hệ thống chưa trích xuất được nội dung văn bản từ tệp; kết quả hiện dùng thông tin biểu mẫu và siêu dữ liệu tệp.");
        if (report.Findings.Count == 0)
            report.Recommendations.Add("Học liệu đạt các tiêu chí tự động theo bằng chứng hiện có. Có thể xuất báo cáo và xác nhận nghiệm thu.");
        FinalizeReport(report);
        return report;
    }

    private static void FinalizeReport(AutoAssessmentReport report)
    {
        report.TechnicalScore = Math.Clamp(report.TechnicalScore, 0, 100);
        report.ContentScore = Math.Clamp(report.ContentScore, 0, 100);
        report.PedagogyScore = Math.Clamp(report.PedagogyScore, 0, 100);
        report.ObeScore = Math.Clamp(report.ObeScore, 0, 100);
        report.OverallScore = (int)Math.Round(report.TechnicalScore * .25 + report.ContentScore * .25 + report.PedagogyScore * .25 + report.ObeScore * .25);
        var hasHighFinding = report.Findings.Any(x => x.Level == "Cao");
        report.Verdict = report.OverallScore >= 80 && !hasHighFinding
            ? "Đạt kiểm thử tự động"
            : "Chưa đạt kiểm thử tự động";
        if (string.IsNullOrWhiteSpace(report.Opinion))
            report.Opinion = report.Verdict == "Đạt kiểm thử tự động"
                ? "Học liệu đáp ứng các kiểm tra tự động ban đầu. Chuyển CCD kiểm tra nội dung thực tế và xác nhận nghiệm thu."
                : "Học liệu cần khắc phục các vấn đề được liệt kê, sau đó nộp phiên bản mới để AI đánh giá lại.";
    }
}
