using DigitalLearningQA.Domain.Common;
using DigitalLearningQA.Domain.Enums;

namespace DigitalLearningQA.Domain.Entities;

public class User : BaseEntity
{
    public Guid RoleId { get; set; }

    public Role Role { get; set; } = null!;

    public Guid DepartmentId { get; set; }

    public Department Department { get; set; } = null!;

    public required string Username { get; set; }

    public required string FullName { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public string? Phone { get; set; }

    public string? Avatar { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public bool IsFirstLogin { get; set; } = true;

    public ICollection<CourseLecturer> CourseLecturers { get; set; }
        = new List<CourseLecturer>();

    public ICollection<LearningMaterial> LearningMaterials { get; set; }
        = new List<LearningMaterial>();

    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();

    public ICollection<AuditLog> AuditLogs { get; set; }
        = new List<AuditLog>();
}