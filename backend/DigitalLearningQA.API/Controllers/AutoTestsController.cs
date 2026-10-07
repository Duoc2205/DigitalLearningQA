using System.Security.Claims;
using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Features.AutoTests.Commands.RunAutoTest;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using DigitalLearningQA.Domain.Enums;

namespace DigitalLearningQA.API.Controllers;

[ApiController]
[Route("api/auto-tests")]
[Authorize]
public class AutoTestsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _db;

    public AutoTestsController(IMediator mediator, IApplicationDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Run(
        RunAutoTestCommand command,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();
        var version = await _db.MaterialVersions.FindAsync([command.MaterialVersionId], cancellationToken);
        if (version is null) return NotFound(new { message = "Không tìm thấy phiên bản học liệu." });
        if (version.UploadedBy != userId && !User.IsInRole("Admin")) return Forbid();
        var id = await _mediator.Send(
            command,
            cancellationToken);

        return Ok(new
        {
            id,
            message = "Đã tạo lượt kiểm thử tự động."
        });
    }

    [HttpGet("{testRequestId:guid}/report")]
    public async Task<IActionResult> GetReport(Guid testRequestId, CancellationToken cancellationToken)
    {
        var test = await _db.TestRequests.Include(x => x.MaterialVersion).ThenInclude(x => x.LearningMaterial)
            .SingleOrDefaultAsync(x => x.Id == testRequestId, cancellationToken);
        if (test is null) return NotFound(new { message = "Không tìm thấy báo cáo kiểm tra tự động." });
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        if (!User.IsInRole("Admin") && !User.IsInRole("CCD") && test.MaterialVersion.LearningMaterial.OwnerId != userId) return Forbid();
        const string prefix = "[AUTO_ASSESSMENT]";
        if (test.Conclusion?.StartsWith(prefix) != true) return NotFound(new { message = "Lượt kiểm thử này chưa có báo cáo đánh giá tự động chi tiết." });
        return Content(test.Conclusion[prefix.Length..], "application/json");
    }

    [HttpPost("{testRequestId:guid}/acceptance")]
    public async Task<IActionResult> AcceptByAiResult(Guid testRequestId, CancellationToken cancellationToken)
    {
        var test = await _db.TestRequests.Include(x => x.MaterialVersion).ThenInclude(x => x.LearningMaterial)
            .SingleOrDefaultAsync(x => x.Id == testRequestId, cancellationToken);
        if (test is null) return NotFound(new { message = "Không tìm thấy báo cáo đánh giá." });
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        var material = test.MaterialVersion.LearningMaterial;
        if (!User.IsInRole("Admin") && !User.IsInRole("CCD") && material.OwnerId != userId) return Forbid();

        const string prefix = "[AUTO_ASSESSMENT]";
        if (test.Conclusion?.StartsWith(prefix) != true) return BadRequest(new { message = "Báo cáo AI chưa sẵn sàng để nghiệm thu." });
        using var report = JsonDocument.Parse(test.Conclusion[prefix.Length..]);
        var root = report.RootElement;
        var score = root.TryGetProperty("OverallScore", out var pascalScore)
            ? pascalScore.GetInt32()
            : root.TryGetProperty("overallScore", out var camelScore) ? camelScore.GetInt32() : 0;
        var hasHigh = root.TryGetProperty("Findings", out var pascalFindings)
            ? pascalFindings.EnumerateArray().Any(x => x.TryGetProperty("Level", out var level) && level.GetString() == "Cao")
            : root.TryGetProperty("findings", out var camelFindings) && camelFindings.EnumerateArray().Any(x => x.TryGetProperty("level", out var level) && level.GetString() == "Cao");
        if (score < 80 || hasHigh)
            return BadRequest(new { message = "Chỉ có thể nghiệm thu khi AI kết luận đạt từ 80 điểm và không còn lỗi mức cao." });

        material.Status = MaterialStatus.Approved;
        material.ApprovedAt = DateTime.UtcNow;
        test.MaterialVersion.Status = MaterialStatus.Approved;
        test.MaterialVersion.ApprovedAt = DateTime.UtcNow;
        _db.Notifications.Add(new DigitalLearningQA.Domain.Entities.Notification
        {
            UserId = material.OwnerId,
            Type = "Accepted",
            Title = "Học liệu đã được nghiệm thu",
            Message = $"{material.Title} đã được xác nhận nghiệm thu theo kết quả AI.",
            Link = "materials"
        });
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Đã nghiệm thu theo kết quả đánh giá AI." });
    }
}
