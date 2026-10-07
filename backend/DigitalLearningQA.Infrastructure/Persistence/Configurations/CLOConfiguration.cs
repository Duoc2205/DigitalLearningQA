using DigitalLearningQA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalLearningQA.Infrastructure.Persistence.Configurations;

public class CLOConfiguration : IEntityTypeConfiguration<CLO>
{
    public void Configure(EntityTypeBuilder<CLO> builder)
    {
        builder.ToTable("CLOs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Weight)
            .HasPrecision(5,2);

        builder.HasOne(x => x.Course)
            .WithMany(x => x.CLOs)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.CourseId,
            x.Code
        }).IsUnique();
    }
}