using DigitalLearningQA.Domain.Common;

namespace DigitalLearningQA.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public required string Type { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
