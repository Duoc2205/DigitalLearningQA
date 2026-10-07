using DigitalLearningQA.API.DTOs.Users;
using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Domain.Entities;
using DigitalLearningQA.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private static readonly string[] AssignableRoles = ["CCD", "Lecturer"];
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogRepository _audit;

    public UsersController(IApplicationDbContext db, IPasswordHasher passwordHasher, IAuditLogRepository audit)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _db.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Include(x => x.Role)
            .Include(x => x.Department)
            .OrderBy(x => x.FullName)
            .Select(x => new
            {
                x.Id,
                x.Username,
                x.FullName,
                x.Email,
                Role = x.Role.Name,
                Department = x.Department.Name,
                x.Status,
                x.IsFirstLogin
            })
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct) => Ok(new
    {
        Total = await _db.Users.CountAsync(x => !x.IsDeleted, ct),
        Active = await _db.Users.CountAsync(x => !x.IsDeleted && x.Status == UserStatus.Active, ct),
        Locked = await _db.Users.CountAsync(x => !x.IsDeleted && x.Status == UserStatus.Locked, ct),
        Ccd = await _db.Users.Include(x => x.Role).CountAsync(x => !x.IsDeleted && x.Role.Name == "CCD", ct),
        Lecturers = await _db.Users.Include(x => x.Role).CountAsync(x => !x.IsDeleted && x.Role.Name == "Lecturer", ct)
    });

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (!AssignableRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "Chỉ được cấp tài khoản cho CCD hoặc Giảng viên." });

        if (await _db.Users.AnyAsync(x => !x.IsDeleted && (x.Username == request.Username || x.Email == request.Email), cancellationToken))
            return Conflict(new { message = "Tên đăng nhập hoặc email đã tồn tại." });

        var role = await _db.Roles.SingleOrDefaultAsync(
            x => x.Name == request.Role,
            cancellationToken);
        var departmentExists = await _db.Departments.AnyAsync(
            x => x.Id == request.DepartmentId && x.IsActive,
            cancellationToken);

        if (role is null || !departmentExists)
            return BadRequest(new { message = "Vai trò hoặc khoa/đơn vị không hợp lệ." });

        var user = new User
        {
            RoleId = role.Id,
            DepartmentId = request.DepartmentId,
            Username = request.Username.Trim(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Status = UserStatus.Active,
            IsFirstLogin = true
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.AddAsync(new AuditLog { Id = Guid.NewGuid(), UserId = user.Id, Action = "CREATE_USER", Description = $"Admin đã cấp tài khoản {user.Username} ({role.Name}).", CreatedAt = DateTime.UtcNow });

        return CreatedAtAction(nameof(GetAll), new { user.Id }, new
        {
            user.Id,
            user.Username,
            user.FullName,
            Role = role.Name,
            message = "Đã cấp tài khoản. Người dùng cần đổi mật khẩu ở lần đăng nhập đầu tiên."
        });
    }

    [HttpPut("{id:guid}/lock")]
    public async Task<IActionResult> SetLock(Guid id, [FromQuery] bool locked, CancellationToken ct)
    {
        var user = await _db.Users.Include(x => x.Role).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (user is null) return NotFound(new { message = "Không tìm thấy tài khoản." });
        if (user.Role.Name == "Admin") return BadRequest(new { message = "Không thể khóa tài khoản Quản trị viên." });
        user.Status = locked ? UserStatus.Locked : UserStatus.Active;
        await _db.SaveChangesAsync(ct);
        await _audit.AddAsync(new AuditLog { Id = Guid.NewGuid(), UserId = user.Id, Action = locked ? "LOCK_USER" : "UNLOCK_USER", Description = locked ? $"Đã khóa tài khoản {user.Username}." : $"Đã mở khóa tài khoản {user.Username}.", CreatedAt = DateTime.UtcNow });
        return Ok(new { message = locked ? "Đã khóa tài khoản." : "Đã mở khóa tài khoản." });
    }

    [HttpPut("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, ResetPasswordRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự." });
        var user = await _db.Users.Include(x => x.Role).SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (user is null) return NotFound(new { message = "Không tìm thấy tài khoản." });
        if (user.Role.Name == "Admin") return BadRequest(new { message = "Không đặt lại mật khẩu Admin từ màn hình này." });
        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.Status = UserStatus.Active;
        user.IsFirstLogin = true;
        await _db.SaveChangesAsync(ct);
        await _audit.AddAsync(new AuditLog { Id = Guid.NewGuid(), UserId = user.Id, Action = "RESET_PASSWORD", Description = $"Admin đã đặt lại mật khẩu cho tài khoản {user.Username}.", CreatedAt = DateTime.UtcNow });
        return Ok(new { message = "Đã đặt lại mật khẩu và mở khóa tài khoản." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var user = await _db.Users.Include(x => x.Role).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (user is null) return NotFound(new { message = "Không tìm thấy tài khoản." });
        if (user.Role.Name == "Admin") return BadRequest(new { message = "Không thể xóa tài khoản Quản trị viên." });
        // Xóa mềm: vẫn giữ nguyên chủ sở hữu học liệu, checklist, phiếu lỗi và nhật ký để báo cáo nghiệm thu không bị mất.
        // Tài khoản biến mất khỏi danh sách quản trị và không thể đăng nhập.
        user.IsDeleted = true;
        user.Status = UserStatus.Locked;
        user.Username = $"deleted-{user.Id:N}";
        user.Email = $"deleted-{user.Id:N}@deleted.local";
        user.FullName = "Tài khoản đã xóa";
        user.Phone = null;
        user.Avatar = null;
        await _db.SaveChangesAsync(ct);
        await _audit.AddAsync(new AuditLog { Id = Guid.NewGuid(), UserId = null, Action = "DELETE_USER", Description = $"Đã xóa tài khoản {user.Username}.", CreatedAt = DateTime.UtcNow });
        return Ok(new { message = "Đã xóa tài khoản." });
    }
}

public sealed class ResetPasswordRequest { public required string NewPassword { get; init; } }
