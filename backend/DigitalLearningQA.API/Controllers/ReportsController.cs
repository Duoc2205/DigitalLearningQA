using DigitalLearningQA.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Claims;
using DigitalLearningQA.Application.Features.AutoTests;
using System.Text.Json;

namespace DigitalLearningQA.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin,CCD")]
public class ReportsController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    public ReportsController(IApplicationDbContext db) => _db = db;

    [HttpGet("overview")]
    public async Task<IActionResult> Overview(CancellationToken ct)
    {
        var byDepartment = await _db.LearningMaterials.AsNoTracking()
            .Include(x => x.Course).ThenInclude(x => x.Department)
            .GroupBy(x => x.Course.Department.Name)
            .Select(x => new { Department = x.Key, Total = x.Count(), NeedRevision = x.Count(m => (int)m.Status == 5), Approved = x.Count(m => (int)m.Status == 6) })
            .OrderByDescending(x => x.Total).ToListAsync(ct);
        return Ok(new
        {
            Materials = await _db.LearningMaterials.CountAsync(ct),
            PendingApproval = await _db.LearningMaterials.CountAsync(x => (int)x.Status == 4, ct),
            NeedRevision = await _db.LearningMaterials.CountAsync(x => (int)x.Status == 5, ct),
            Approved = await _db.LearningMaterials.CountAsync(x => (int)x.Status == 6, ct),
            TestRequests = await _db.TestRequests.CountAsync(ct),
            ByDepartment = byDepartment
        });
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> AuditLogs([FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 200);
        var logs = await _db.AuditLogs.AsNoTracking().Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt).Take(take)
            .Select(x => new { x.Id, x.Action, x.Description, x.CreatedAt, User = x.User == null ? "Hệ thống" : x.User.FullName })
            .ToListAsync(ct);
        return Ok(logs);
    }

    [HttpGet("export/acceptance-csv")]
    public async Task<IActionResult> ExportAcceptanceCsv(CancellationToken ct)
    {
        var materials = await _db.LearningMaterials.AsNoTracking()
            .Include(x => x.Course).ThenInclude(x => x.Department)
            .Include(x => x.Course).ThenInclude(x => x.Major)
            .Include(x => x.Owner)
            .OrderBy(x => x.Course.Department.Name).ThenBy(x => x.Course.Code).ThenBy(x => x.Code)
            .Select(x => new { x.Code, x.Title, Department = x.Course.Department.Name, Major = x.Course.Major == null ? "" : x.Course.Major.Name, CourseCode = x.Course.Code, CourseName = x.Course.Name, Owner = x.Owner.FullName, Status = x.Status, x.CreatedAt })
            .ToListAsync(ct);
        var status = new[] { "Nháp", "Đã tải", "Đang xử lý", "Đã kiểm thử", "Chờ duyệt", "Cần chỉnh sửa", "Đã nghiệm thu" };
        static string Csv(string? value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
        var csv = new StringBuilder("Mã học liệu,Tên học liệu,Khoa,Ngành,Mã học phần,Học phần,Người nộp,Trạng thái,Ngày tạo\r\n");
        foreach (var x in materials)
            csv.AppendJoin(',', Csv(x.Code), Csv(x.Title), Csv(x.Department), Csv(x.Major), Csv(x.CourseCode), Csv(x.CourseName), Csv(x.Owner), Csv(status.ElementAtOrDefault((int)x.Status) ?? x.Status.ToString()), Csv(x.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"))).Append("\r\n");
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(), "text/csv; charset=utf-8", $"bao-cao-nghiem-thu-{DateTime.Now:yyyyMMdd}.csv");
    }

    [HttpGet("materials/{materialId:guid}/acceptance-csv")]
    [Authorize]
    public async Task<IActionResult> ExportMaterialAcceptanceCsv(Guid materialId, CancellationToken ct)
    {
        var material = await _db.LearningMaterials.AsNoTracking().Include(x => x.Course).ThenInclude(x => x.Department)
            .Include(x => x.Course).ThenInclude(x => x.Major).Include(x => x.Owner)
            .SingleOrDefaultAsync(x => x.Id == materialId, ct);
        if (material is null) return NotFound(new { message = "Không tìm thấy học liệu." });
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        if (!User.IsInRole("Admin") && !User.IsInRole("CCD") && material.OwnerId != userId) return Forbid();
        var test = await _db.TestRequests.AsNoTracking().OrderByDescending(x => x.RequestedDate)
            .FirstOrDefaultAsync(x => x.MaterialVersionId == material.CurrentVersionId, ct);
        static string Csv(string? value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
        AutoAssessmentReport? autoReport = null;
        const string autoPrefix = "[AUTO_ASSESSMENT]";
        if (test?.Conclusion?.StartsWith(autoPrefix) == true)
        {
            try { autoReport = JsonSerializer.Deserialize<AutoAssessmentReport>(test.Conclusion[autoPrefix.Length..]); }
            catch (JsonException) { /* Báo cáo cũ không hợp lệ vẫn có thể xuất phần nghiệm thu. */ }
        }
        var sb = new StringBuilder();
        sb.AppendLine("BÁO CÁO ĐÁNH GIÁ HỌC LIỆU SỐ");
        sb.AppendLine($"Mã học liệu,{Csv(material.Code)}"); sb.AppendLine($"Tên học liệu,{Csv(material.Title)}");
        sb.AppendLine($"Học phần,{Csv(material.Course.Code + " - " + material.Course.Name)}"); sb.AppendLine($"Khoa,{Csv(material.Course.Department.Name)}");
        sb.AppendLine($"Ngành,{Csv(material.Course.Major?.Name)}"); sb.AppendLine($"Người nộp,{Csv(material.Owner.FullName)}");
        if (autoReport is not null)
        {
            sb.AppendLine(); sb.AppendLine("KẾT QUẢ ĐÁNH GIÁ TỰ ĐỘNG");
            sb.AppendLine($"Kết quả,{Csv(autoReport.Verdict)}");
            sb.AppendLine($"Nhận xét AI,{Csv(autoReport.Opinion)}");
            sb.AppendLine($"Điểm tổng,{autoReport.OverallScore}/100");
            sb.AppendLine($"Điểm kỹ thuật,{autoReport.TechnicalScore}/100"); sb.AppendLine($"Điểm nội dung,{autoReport.ContentScore}/100");
            sb.AppendLine($"Điểm sư phạm,{autoReport.PedagogyScore}/100"); sb.AppendLine($"Điểm OBE,{autoReport.ObeScore}/100");
            sb.AppendLine($"Phương pháp đánh giá,{Csv(autoReport.AssessmentMethod)}");
            sb.AppendLine($"Trích xuất nội dung tệp,{(autoReport.FileTextExtracted ? "Có" : "Không")}");
            sb.AppendLine($"Số ký tự trích xuất,{autoReport.ExtractedCharacterCount}");
            sb.AppendLine(); sb.AppendLine("VẤN ĐỀ CẦN KHẮC PHỤC"); sb.AppendLine("Nhóm,Mức độ,Vấn đề,Cách thực hiện");
            foreach (var finding in autoReport.Findings)
                sb.AppendJoin(',', Csv(finding.Group), Csv(finding.Level), Csv(finding.Problem), Csv(finding.Solution)).AppendLine();
            sb.AppendLine(); sb.AppendLine("KIẾN NGHỊ AI"); sb.AppendLine("Nội dung");
            foreach (var recommendation in autoReport.Recommendations) sb.AppendLine(Csv(recommendation));
        }
        else sb.AppendLine($"Kết luận kiểm thử,{Csv(test?.Conclusion)}");
        sb.AppendLine(); sb.AppendLine("KẾT LUẬN NGHIỆM THU");
        sb.AppendLine($"Trạng thái hệ thống,{Csv(material.Status.ToString())}");
        sb.AppendLine("Ghi chú,Quy trình này sử dụng kết quả AI tự động; không yêu cầu checklist hoặc phiếu lỗi thủ công.");
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray(), "text/csv; charset=utf-8", $"nghiem-thu-{material.Code}.csv");
    }
}
