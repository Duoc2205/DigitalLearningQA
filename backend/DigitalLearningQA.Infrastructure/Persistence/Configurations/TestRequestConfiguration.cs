using DigitalLearningQA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalLearningQA.Infrastructure.Persistence.Configurations;

public class TestRequestConfiguration : IEntityTypeConfiguration<TestRequest>
{
    public void Configure(EntityTypeBuilder<TestRequest> builder)
    {
        builder.ToTable("TestRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RequestedDate)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()   // Lưu enum dưới dạng int
            .IsRequired();

        // Báo cáo AI chứa điểm, lỗi và khuyến nghị chi tiết nên không giới hạn 2.000 ký tự.
        builder.Property(x => x.Conclusion)
            .HasColumnType("nvarchar(max)");

        // TestRequest -> MaterialVersion (1-n)
        builder.HasOne(x => x.MaterialVersion)
            .WithMany(v => v.TestRequests)
            .HasForeignKey(x => x.MaterialVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        // TestRequest -> Tester (User)
        builder.HasOne(x => x.Tester)
            .WithMany()
            .HasForeignKey(x => x.TesterId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
