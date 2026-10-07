using DigitalLearningQA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Role> Roles { get; }

    DbSet<User> Users { get; }

    DbSet<Department> Departments { get; }

    DbSet<Major> Majors { get; }

    DbSet<QualityCriterion> QualityCriteria { get; }

    DbSet<ChecklistReview> ChecklistReviews { get; }

    DbSet<QualityIssue> QualityIssues { get; }

    DbSet<Course> Courses { get; }

    DbSet<CourseLecturer> CourseLecturers { get; }

    DbSet<LearningMaterial> LearningMaterials { get; }

    DbSet<MaterialVersion> MaterialVersions { get; }

    DbSet<TestRequest> TestRequests { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<AuditLog> AuditLogs { get; }

    DbSet<Notification> Notifications { get; }

    DbSet<CLO> CLOs { get; }

    DbSet<LearningMaterialCLO> LearningMaterialCLOs { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
