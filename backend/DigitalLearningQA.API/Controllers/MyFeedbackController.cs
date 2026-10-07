using System.Security.Claims;
using DigitalLearningQA.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.API.Controllers;

[ApiController]
[Route("api/my-feedback")]
[Authorize]
public class MyFeedbackController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    public MyFeedbackController(IApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        var query = _db.TestRequests.AsNoTracking()
            .Include(x => x.MaterialVersion).ThenInclude(x => x.LearningMaterial)
            .AsQueryable();

        // Giảng viên chỉ xem học liệu của mình; CCD và Admin xem toàn bộ báo cáo AI để kiểm tra/nghiệm thu.
        if (User.IsInRole("Lecturer"))
            query = query.Where(x => x.MaterialVersion.LearningMaterial.OwnerId == userId);

        var feedback = await query
            .OrderByDescending(x => x.RequestedDate)
            .Select(x => new
            {
                x.Id, MaterialId = x.MaterialVersion.LearningMaterialId, Material = x.MaterialVersion.LearningMaterial.Title,
                x.MaterialVersion.VersionNumber, x.Status, x.Conclusion, x.RequestedDate, x.CompletedDate,
                Issues = _db.QualityIssues.Where(i => i.TestRequestId == x.Id).OrderByDescending(i => i.CreatedDate)
                    .Select(i => new { i.Id, i.Title, i.Category, i.Severity, i.Description, i.Status, i.DueDate, i.ResolutionNote }).ToList()
            }).ToListAsync(ct);
        return Ok(feedback);
    }
}
