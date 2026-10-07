using System.Security.Claims;
using DigitalLearningQA.API.DTOs.LearningMaterials;
using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Features.LearningMaterials.Commands;
using DigitalLearningQA.Application.Features.LearningMaterials.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.API.Controllers;

[ApiController]
[Route("api/learning-materials")]
[Authorize]
public class LearningMaterialsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearningMaterialsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllLearningMaterialsQuery());
        if (User.IsInRole("Lecturer") && Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            result = result.Where(x => x.OwnerId == userId).ToList();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "CCD,Lecturer")]
    public async Task<IActionResult> Create(CreateLearningMaterialRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var ownerId))
            return Unauthorized();
        var id = await _mediator.Send(new CreateLearningMaterialCommand(
            request.Code, request.CourseId, ownerId, request.Title, request.Description, request.MaterialType));

        return Ok(new
        {
            Id = id,
            Message = "Đã tạo học liệu thành công."
        });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateLearningMaterialRequest request, [FromServices] IApplicationDbContext db, CancellationToken ct)
    {
        var material = await db.LearningMaterials.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (material is null) return NotFound(new { message = "Không tìm thấy học liệu." });
        if (!await db.Courses.AnyAsync(x => x.Id == request.CourseId && x.IsActive, ct)) return BadRequest(new { message = "Học phần không hợp lệ." });
        if (!Enum.IsDefined(typeof(DigitalLearningQA.Domain.Enums.MaterialType), request.MaterialType)) return BadRequest(new { message = "Loại học liệu không hợp lệ." });
        material.CourseId = request.CourseId; material.Title = request.Title.Trim(); material.Description = request.Description?.Trim(); material.MaterialType = (DigitalLearningQA.Domain.Enums.MaterialType)request.MaterialType;
        await db.SaveChangesAsync(ct); return Ok(new { message = "Đã cập nhật học liệu." });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, [FromServices] IApplicationDbContext db, CancellationToken ct)
    {
        var material = await db.LearningMaterials.Include(x => x.Versions).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (material is null) return NotFound(new { message = "Không tìm thấy học liệu." });
        material.CurrentVersionId = null; await db.SaveChangesAsync(ct);
        db.LearningMaterials.Remove(material); await db.SaveChangesAsync(ct);
        return Ok(new { message = "Đã xóa học liệu và các phiên bản liên quan." });
    }
}
