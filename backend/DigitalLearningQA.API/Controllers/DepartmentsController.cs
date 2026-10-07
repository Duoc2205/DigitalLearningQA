using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.API.DTOs.Departments;
using DigitalLearningQA.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.API.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    public DepartmentsController(IApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) => Ok(
        await _db.Departments.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Code, x.Name })
            .ToListAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateDepartmentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Mã và tên khoa là bắt buộc." });
        if (await _db.Departments.AnyAsync(x => x.Code == request.Code, ct))
            return Conflict(new { message = "Mã khoa đã tồn tại." });
        var department = new Department { Code = request.Code.Trim(), Name = request.Name.Trim(), Description = request.Description?.Trim(), IsActive = true };
        _db.Departments.Add(department); await _db.SaveChangesAsync(ct);
        return Ok(new { department.Id, department.Code, department.Name });
    }
}
