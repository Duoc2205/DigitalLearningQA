using DigitalLearningQA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalLearningQA.Infrastructure.Persistence.Configurations;

public class MaterialVersionConfiguration
    : IEntityTypeConfiguration<MaterialVersion>
{
    public void Configure(EntityTypeBuilder<MaterialVersion> builder)
    {
        builder.ToTable("MaterialVersions");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.LearningMaterialId, x.VersionNumber }).IsUnique();

        builder.Property(x => x.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.FilePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.MimeType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.StorageProvider)
            .HasMaxLength(50);

        builder.Property(x => x.Remark)
            .HasMaxLength(1000);

        // LearningMaterial (1 - N)
        builder.HasOne(x => x.LearningMaterial)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.LearningMaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        // User upload
        builder.HasOne(x => x.UploadedUser)
            .WithMany()
            .HasForeignKey(x => x.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
