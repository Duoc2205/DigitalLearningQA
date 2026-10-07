using DigitalLearningQA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalLearningQA.Infrastructure.Persistence.Configurations;

public class ChecklistReviewConfiguration : IEntityTypeConfiguration<ChecklistReview>
{
    public void Configure(EntityTypeBuilder<ChecklistReview> builder)
    {
        builder.ToTable("ChecklistReviews"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Outcome).HasMaxLength(30).IsRequired(); builder.Property(x => x.Note).HasMaxLength(2000);
        builder.HasIndex(x => new { x.TestRequestId, x.QualityCriterionId }).IsUnique();
        builder.HasOne(x => x.TestRequest).WithMany().HasForeignKey(x => x.TestRequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.QualityCriterion).WithMany().HasForeignKey(x => x.QualityCriterionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ReviewedBy).WithMany().HasForeignKey(x => x.ReviewedById).OnDelete(DeleteBehavior.Restrict);
    }
}
