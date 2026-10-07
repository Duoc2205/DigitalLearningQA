using DigitalLearningQA.API.DTOs.Majors;
using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.API.Controllers;

[ApiController]
[Route("api/majors")]
[Authorize]
public class MajorsController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    public MajorsController(IApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? departmentId, CancellationToken ct) => Ok(
        await _db.Majors.AsNoTracking().Where(x => x.IsActive && (!departmentId.HasValue || x.DepartmentId == departmentId))
            .OrderBy(x => x.Name).Select(x => new { x.Id, x.DepartmentId, x.Code, x.Name }).ToListAsync(ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateMajorRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Mã và tên ngành là bắt buộc." });
        if (!await _db.Departments.AnyAsync(x => x.Id == request.DepartmentId && x.IsActive, ct)) return BadRequest(new { message = "Khoa không hợp lệ." });
        if (await _db.Majors.AnyAsync(x => x.DepartmentId == request.DepartmentId && x.Code == request.Code, ct)) return Conflict(new { message = "Mã ngành đã tồn tại trong khoa này." });
        var major = new Major { DepartmentId = request.DepartmentId, Code = request.Code.Trim(), Name = request.Name.Trim() };
        _db.Majors.Add(major); await _db.SaveChangesAsync(ct);
        return Ok(new { major.Id, major.DepartmentId, major.Code, major.Name });
    }
}
