using System.Security.Claims;
using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Domain.Entities;
using DigitalLearningQA.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.API.Controllers;

[ApiController]
[Route("api/ccd/reviews")]
[Authorize(Roles = "CCD")]
public class CcdReviewsController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    public CcdReviewsController(IApplicationDbContext db) => _db = db;
    private bool CurrentUser(out Guid id) => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out id);

    [HttpGet("queue")]
    public async Task<IActionResult> Queue(CancellationToken ct) => Ok(await _db.TestRequests.AsNoTracking()
        .Include(x => x.MaterialVersion).ThenInclude(x => x.LearningMaterial).ThenInclude(x => x.Course).ThenInclude(x => x.Department)
        .Include(x => x.MaterialVersion).ThenInclude(x => x.LearningMaterial).ThenInclude(x => x.Owner)
        .Where(x => x.MaterialVersion.LearningMaterial.CurrentVersionId == x.MaterialVersionId)
        .OrderBy(x => x.MaterialVersion.LearningMaterial.Status == MaterialStatus.PendingApproval ? 0 : 1).ThenByDescending(x => x.RequestedDate)
        .Select(x => new { x.Id, x.Status, x.Conclusion, x.RequestedDate, x.CompletedDate, x.MaterialVersionId, x.MaterialVersion.VersionNumber, x.MaterialVersion.FileName,
            MaterialId = x.MaterialVersion.LearningMaterialId, MaterialCode = x.MaterialVersion.LearningMaterial.Code, MaterialTitle = x.MaterialVersion.LearningMaterial.Title,
            MaterialType = x.MaterialVersion.LearningMaterial.MaterialType, MaterialStatus = x.MaterialVersion.LearningMaterial.Status, Course = x.MaterialVersion.LearningMaterial.Course.Name,
            Department = x.MaterialVersion.LearningMaterial.Course.Department.Name, Lecturer = x.MaterialVersion.LearningMaterial.Owner.FullName })
        .ToListAsync(ct));

    [HttpGet("{testRequestId:guid}")]
    public async Task<IActionResult> Detail(Guid testRequestId, CancellationToken ct)
    {
        var test = await _db.TestRequests.AsNoTracking().Include(x => x.MaterialVersion).ThenInclude(x => x.LearningMaterial)
            .SingleOrDefaultAsync(x => x.Id == testRequestId, ct);
        if (test is null) return NotFound(new { message = "Không tìm thấy lượt kiểm thử." });
        var criteria = await _db.QualityCriteria.AsNoTracking().Where(x => x.IsActive && (x.MaterialType == null || x.MaterialType == test.MaterialVersion.LearningMaterial.MaterialType))
            .OrderBy(x => x.Category).ThenBy(x => x.Code).Select(x => new { x.Id, x.Code, x.Name, x.Category, x.Description, x.IsRequired, x.Weight }).ToListAsync(ct);
        var reviews = await _db.ChecklistReviews.AsNoTracking().Where(x => x.TestRequestId == testRequestId)
            .Select(x => new { x.QualityCriterionId, x.Outcome, x.Note, x.ReviewedAt }).ToListAsync(ct);
        var issues = await _db.QualityIssues.AsNoTracking().Where(x => x.TestRequestId == testRequestId).OrderByDescending(x => x.CreatedDate)
            .Select(x => new { x.Id, x.Title, x.Category, x.Severity, x.Description, x.Status, x.EvidenceUrl, x.DueDate, x.CreatedDate, x.ResolutionNote }).ToListAsync(ct);
        return Ok(new { Criteria = criteria, Reviews = reviews, Issues = issues });
    }

    [HttpPut("{testRequestId:guid}/checklist")]
    public async Task<IActionResult> SaveChecklist(Guid testRequestId, List<ChecklistReviewRequest> items, CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Unauthorized();
        if (!await _db.TestRequests.AnyAsync(x => x.Id == testRequestId, ct)) return NotFound(new { message = "Không tìm thấy lượt kiểm thử." });
        var outcomes = new[] { "Dat", "CanChinhSua", "KhongApDung" };
        foreach (var item in items)
        {
            if (!outcomes.Contains(item.Outcome)) return BadRequest(new { message = "Kết quả checklist không hợp lệ." });
            var review = await _db.ChecklistReviews.SingleOrDefaultAsync(x => x.TestRequestId == testRequestId && x.QualityCriterionId == item.QualityCriterionId, ct);
            if (review is null) _db.ChecklistReviews.Add(new ChecklistReview { TestRequestId = testRequestId, QualityCriterionId = item.QualityCriterionId, Outcome = item.Outcome, Note = item.Note?.Trim(), ReviewedById = userId, ReviewedAt = DateTime.UtcNow });
            else { review.Outcome = item.Outcome; review.Note = item.Note?.Trim(); review.ReviewedById = userId; review.ReviewedAt = DateTime.UtcNow; }
        }
        await _db.SaveChangesAsync(ct); return Ok(new { message = "Đã lưu checklist." });
    }

    [HttpPost("{testRequestId:guid}/issues")]
    public async Task<IActionResult> CreateIssue(Guid testRequestId, CreateIssueRequest request, CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description)) return BadRequest(new { message = "Tiêu đề và mô tả lỗi là bắt buộc." });
        if (!await _db.TestRequests.AnyAsync(x => x.Id == testRequestId, ct)) return NotFound(new { message = "Không tìm thấy lượt kiểm thử." });
        var issue = new QualityIssue { TestRequestId = testRequestId, Title = request.Title.Trim(), Category = request.Category ?? "Khác", Severity = request.Severity ?? "TrungBinh", Description = request.Description.Trim(), EvidenceUrl = request.EvidenceUrl?.Trim(), DueDate = request.DueDate, CreatedById = userId, CreatedDate = DateTime.UtcNow };
        _db.QualityIssues.Add(issue); await _db.SaveChangesAsync(ct); return Ok(new { issue.Id, message = "Đã lập phiếu lỗi." });
    }

    [HttpPut("{testRequestId:guid}/decision")]
    public async Task<IActionResult> Decision(Guid testRequestId, ReviewDecisionRequest request, CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Unauthorized();
        if (request.Decision is not ("NeedRevision" or "Approved")) return BadRequest(new { message = "Kết luận không hợp lệ." });
        var test = await _db.TestRequests.Include(x => x.MaterialVersion).ThenInclude(x => x.LearningMaterial).SingleOrDefaultAsync(x => x.Id == testRequestId, ct);
        if (test is null) return NotFound(new { message = "Không tìm thấy lượt kiểm thử." });
        if (test.MaterialVersion.LearningMaterial.OwnerId == userId)
            return BadRequest(new { message = "CCD không thể tự nghiệm thu học liệu do chính mình nộp. Hãy chuyển học liệu cho một CCD khác kiểm tra." });

        var requiredCriteria = await _db.QualityCriteria.Where(x => x.IsActive && x.IsRequired && (x.MaterialType == null || x.MaterialType == test.MaterialVersion.LearningMaterial.MaterialType))
            .Select(x => x.Id).ToListAsync(ct);
        var reviewed = await _db.ChecklistReviews.Where(x => x.TestRequestId == testRequestId).ToListAsync(ct);
        var missingRequired = requiredCriteria.Where(id => !reviewed.Any(x => x.QualityCriterionId == id && (x.Outcome == "Dat" || x.Outcome == "KhongApDung"))).ToList();
        if (request.Decision == "Approved" && missingRequired.Count > 0)
            return BadRequest(new { message = $"Chưa hoàn tất {missingRequired.Count} tiêu chí bắt buộc. Hãy lưu checklist trước khi nghiệm thu." });
        var openIssues = await _db.QualityIssues.AnyAsync(x => x.TestRequestId == testRequestId && x.Status == "Mo", ct);
        if (request.Decision == "Approved" && openIssues) return BadRequest(new { message = "Không thể nghiệm thu khi còn phiếu lỗi đang mở." });
        test.TesterId = userId; test.StartedDate ??= DateTime.UtcNow; test.CompletedDate = DateTime.UtcNow; test.Status = request.Decision == "Approved" ? TestRequestStatus.Completed : TestRequestStatus.Rejected; test.Conclusion = request.Conclusion?.Trim();
        var material = test.MaterialVersion.LearningMaterial; material.Status = request.Decision == "Approved" ? MaterialStatus.Approved : MaterialStatus.NeedRevision; material.ApprovedAt = request.Decision == "Approved" ? DateTime.UtcNow : null;
        await _db.SaveChangesAsync(ct); return Ok(new { message = request.Decision == "Approved" ? "Đã nghiệm thu học liệu." : "Đã gửi yêu cầu chỉnh sửa cho giảng viên." });
    }
}

public class ChecklistReviewRequest { public Guid QualityCriterionId { get; set; } public string Outcome { get; set; } = ""; public string? Note { get; set; } }
public class CreateIssueRequest { public string Title { get; set; } = ""; public string? Category { get; set; } public string? Severity { get; set; } public string Description { get; set; } = ""; public string? EvidenceUrl { get; set; } public DateTime? DueDate { get; set; } }
public class ReviewDecisionRequest { public string Decision { get; set; } = ""; public string? Conclusion { get; set; } }
