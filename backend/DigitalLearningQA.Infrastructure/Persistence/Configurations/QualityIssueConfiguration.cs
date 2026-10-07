using DigitalLearningQA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalLearningQA.Infrastructure.Persistence.Configurations;

public class QualityIssueConfiguration : IEntityTypeConfiguration<QualityIssue>
{
    public void Configure(EntityTypeBuilder<QualityIssue> builder)
    {
        builder.ToTable("QualityIssues"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(255).IsRequired(); builder.Property(x => x.Category).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Severity).HasMaxLength(30).IsRequired(); builder.Property(x => x.Status).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000).IsRequired(); builder.Property(x => x.EvidenceUrl).HasMaxLength(1000); builder.Property(x => x.ResolutionNote).HasMaxLength(2000);
        builder.HasIndex(x => x.TestRequestId);
        builder.HasOne(x => x.TestRequest).WithMany().HasForeignKey(x => x.TestRequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ResolvedByUser).WithMany().HasForeignKey(x => x.ResolvedById).OnDelete(DeleteBehavior.Restrict);
    }
}
