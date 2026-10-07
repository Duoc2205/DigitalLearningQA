using DigitalLearningQA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalLearningQA.Infrastructure.Persistence.Configurations;

public class LearningMaterialConfiguration
    : IEntityTypeConfiguration<LearningMaterial>
{
    public void Configure(EntityTypeBuilder<LearningMaterial> builder)
    {
        builder.ToTable("LearningMaterials");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(300);

        // Course
        builder.HasOne(x => x.Course)
            .WithMany(x => x.LearningMaterials)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Owner
        builder.HasOne(x => x.Owner)
            .WithMany(x => x.LearningMaterials)
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Danh sách Version
        builder.HasMany(x => x.Versions)
            .WithOne(x => x.LearningMaterial)
            .HasForeignKey(x => x.LearningMaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        // Version hiện tại
        builder.HasOne(x => x.CurrentVersion)
            .WithMany()
            .HasForeignKey(x => x.CurrentVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}