using System.Security.Claims;
using DigitalLearningQA.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    public NotificationsController(IApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int take = 20, CancellationToken ct = default)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        take = Math.Clamp(take, 1, 50);
        var rows = await _db.Notifications.AsNoTracking().Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt).Take(take)
            .Select(x => new { x.Id, x.Type, x.Title, x.Message, x.Link, x.IsRead, x.CreatedAt }).ToListAsync(ct);
        return Ok(rows);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        var notification = await _db.Notifications.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
        if (notification is null) return NotFound(new { message = "Không tìm thấy thông báo." });
        notification.IsRead = true; notification.ReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        var rows = await _db.Notifications.Where(x => x.UserId == userId && !x.IsRead).ToListAsync(ct);
        foreach (var row in rows) { row.IsRead = true; row.ReadAt = DateTime.UtcNow; }
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
