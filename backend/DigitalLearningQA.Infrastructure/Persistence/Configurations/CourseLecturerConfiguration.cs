using DigitalLearningQA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalLearningQA.Infrastructure.Persistence.Configurations;

public class CourseLecturerConfiguration
    : IEntityTypeConfiguration<CourseLecturer>
{
    public void Configure(EntityTypeBuilder<CourseLecturer> builder)
    {
        builder.ToTable("CourseLecturers");

        builder.HasKey(x => new
        {
            x.CourseId,
            x.UserId
        });

        builder.HasOne(x => x.Course)
            .WithMany(x => x.CourseLecturers)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.CourseLecturers)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}