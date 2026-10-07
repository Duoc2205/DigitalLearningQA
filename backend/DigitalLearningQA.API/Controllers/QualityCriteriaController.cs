using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Domain.Entities;
using DigitalLearningQA.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.API.Controllers;

[ApiController]
[Route("api/quality-criteria")]
[Authorize(Roles = "Admin")]
public class QualityCriteriaController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    public QualityCriteriaController(IApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await _db.QualityCriteria.AsNoTracking()
        .OrderBy(x => x.Category).ThenBy(x => x.Code)
        .Select(x => new { x.Id, x.Code, x.Name, x.Category, x.Description, x.MaterialType, x.Weight, x.IsRequired, x.IsActive })
        .ToListAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create(CriterionRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Category))
            return BadRequest(new { message = "Mã, tên và nhóm tiêu chí là bắt buộc." });
        if (await _db.QualityCriteria.AnyAsync(x => x.Code == request.Code.Trim(), ct))
            return Conflict(new { message = "Mã tiêu chí đã tồn tại." });
        var item = new QualityCriterion { Code = request.Code.Trim(), Name = request.Name.Trim(), Category = request.Category.Trim(), Description = request.Description?.Trim(), MaterialType = request.MaterialType, Weight = Math.Max(1, request.Weight), IsRequired = request.IsRequired, IsActive = true };
        _db.QualityCriteria.Add(item); await _db.SaveChangesAsync(ct); return Ok(new { item.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CriterionRequest request, CancellationToken ct)
    {
        var item = await _db.QualityCriteria.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return NotFound(new { message = "Không tìm thấy tiêu chí." });
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Category)) return BadRequest(new { message = "Tên và nhóm tiêu chí là bắt buộc." });
        item.Name = request.Name.Trim(); item.Category = request.Category.Trim(); item.Description = request.Description?.Trim(); item.MaterialType = request.MaterialType; item.Weight = Math.Max(1, request.Weight); item.IsRequired = request.IsRequired; item.IsActive = request.IsActive;
        await _db.SaveChangesAsync(ct); return Ok(new { message = "Đã cập nhật tiêu chí." });
    }
}

public class CriterionRequest
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? Description { get; set; }
    public MaterialType? MaterialType { get; set; }
    public int Weight { get; set; } = 1;
    public bool IsRequired { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
