using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Domain.Common;
using DigitalLearningQA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.Infrastructure.Persistence;

public class ApplicationDbContext
    : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // =========================
    // DbSets
    // =========================

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Major> Majors => Set<Major>();

    public DbSet<QualityCriterion> QualityCriteria => Set<QualityCriterion>();

    public DbSet<ChecklistReview> ChecklistReviews => Set<ChecklistReview>();

    public DbSet<QualityIssue> QualityIssues => Set<QualityIssue>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<CourseLecturer> CourseLecturers => Set<CourseLecturer>();

    public DbSet<LearningMaterial> LearningMaterials => Set<LearningMaterial>();

    public DbSet<MaterialVersion> MaterialVersions => Set<MaterialVersion>();

    public DbSet<TestRequest> TestRequests => Set<TestRequest>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<CLO> CLOs => Set<CLO>();

    public DbSet<LearningMaterialCLO> LearningMaterialCLOs
        => Set<LearningMaterialCLO>();

    // =========================
    // Fluent API
    // =========================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }

    // =========================
    // SaveChanges
    // =========================

    public override int SaveChanges()
    {
        UpdateAuditFields();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();

        return base.SaveChangesAsync(cancellationToken);
    }

    // =========================
    // Audit
    // =========================

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Id = Guid.NewGuid();
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}
