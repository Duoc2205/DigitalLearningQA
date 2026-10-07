using System.Security.Claims;
using DigitalLearningQA.API.DTOs.MaterialVersions;
using DigitalLearningQA.Application.Common.Constants;
using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Features.MaterialVersions.Commands;
using DigitalLearningQA.Application.Features.MaterialVersions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.API.Controllers;

[ApiController]
[Route("api/material-versions")]
[Authorize]
public class MaterialVersionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _db;

    public MaterialVersionsController(IMediator mediator, IApplicationDbContext db) { _mediator = mediator; _db = db; }

    [HttpGet("{learningMaterialId:guid}")]
    public async Task<IActionResult> GetByMaterial(Guid learningMaterialId, CancellationToken ct)
    {
        if (learningMaterialId == Guid.Empty)
            return BadRequest("Mã học liệu là bắt buộc.");
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        var ownerId = await _db.LearningMaterials.Where(x => x.Id == learningMaterialId).Select(x => (Guid?)x.OwnerId).SingleOrDefaultAsync(ct);
        if (ownerId is null) return NotFound(new { message = "Không tìm thấy học liệu." });
        if (!User.IsInRole("Admin") && !User.IsInRole("CCD") && ownerId != userId) return Forbid();
        return Ok(await _mediator.Send(new GetMaterialVersionsQuery { LearningMaterialId = learningMaterialId }, ct));
    }

    [HttpGet("{learningMaterialId:guid}/assessment-history")]
    public async Task<IActionResult> GetAssessmentHistory(Guid learningMaterialId, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        var ownerId = await _db.LearningMaterials.Where(x => x.Id == learningMaterialId)
            .Select(x => (Guid?)x.OwnerId).SingleOrDefaultAsync(ct);
        if (ownerId is null) return NotFound(new { message = "Không tìm thấy học liệu." });
        if (!User.IsInRole("Admin") && !User.IsInRole("CCD") && ownerId != userId) return Forbid();

        var history = await _db.MaterialVersions.AsNoTracking()
            .Where(x => x.LearningMaterialId == learningMaterialId)
            .OrderByDescending(x => x.VersionNumber)
            .Select(x => new
            {
                x.Id, x.VersionNumber, x.FileName, x.UploadDate, x.Remark, x.Status,
                Test = x.TestRequests.OrderByDescending(t => t.CompletedDate ?? t.RequestedDate)
                    .Select(t => new { t.Id, t.Conclusion, t.CompletedDate }).FirstOrDefault()
            })
            .ToListAsync(ct);
        return Ok(history);
    }

    [HttpPost("upload")]
    [RequestSizeLimit(FileSettings.MaxFileSize + 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] UploadMaterialVersionRequest request)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest("Vui lòng chọn tệp học liệu.");
        if (request.LearningMaterialId == Guid.Empty)
            return BadRequest("Vui lòng chọn học liệu.");
        if (request.File.Length > FileSettings.MaxFileSize)
            return BadRequest("Dung lượng tệp vượt quá giới hạn cho phép.");
        if (request.Remark?.Length > 1000)
            return BadRequest("Ghi chú không được vượt quá 1.000 ký tự.");
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var fileName = Path.GetFileName(request.File.FileName);
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 255)
            return BadRequest("Tên tệp không hợp lệ.");

        await using var stream = request.File.OpenReadStream();
        var id = await _mediator.Send(new UploadMaterialVersionCommand
        {
            LearningMaterialId = request.LearningMaterialId,
            UploadedBy = userId,
            FileName = fileName,
            MimeType = request.File.ContentType,
            FileSize = request.File.Length,
            FileStream = stream,
            Remark = request.Remark
        });

        return Ok(new { Id = id, Message = "Đã tải phiên bản học liệu thành công." });
    }
}
