using DigitalLearningQA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalLearningQA.Infrastructure.Persistence.Configurations;

public class LearningMaterialCLOConfiguration
    : IEntityTypeConfiguration<LearningMaterialCLO>
{
    public void Configure(EntityTypeBuilder<LearningMaterialCLO> builder)
    {
        builder.ToTable("LearningMaterialCLOs");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.LearningMaterial)
            .WithMany(x => x.LearningMaterialCLOs)
            .HasForeignKey(x => x.LearningMaterialId);

        builder.HasOne(x => x.CLO)
            .WithMany(x => x.LearningMaterialCLOs)
            .HasForeignKey(x => x.CLOId);

        builder.HasIndex(x => new
        {
            x.LearningMaterialId,
            x.CLOId
        }).IsUnique();
    }
}