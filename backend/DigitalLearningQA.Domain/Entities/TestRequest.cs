using DigitalLearningQA.Domain.Enums;

namespace DigitalLearningQA.Domain.Entities;

public class TestRequest
{
    public Guid Id { get; set; }

    // Phiên bản học liệu được gửi kiểm thử
    public Guid MaterialVersionId { get; set; }

    // Người được phân công kiểm thử
    // Null khi đây là lượt kiểm thử tự động do AI/hệ thống thực hiện.
    public Guid? TesterId { get; set; }

    // Ngày tạo yêu cầu
    public DateTime RequestedDate { get; set; }

    // Ngày tester bắt đầu kiểm thử
    public DateTime? StartedDate { get; set; }

    // Ngày hoàn thành kiểm thử
    public DateTime? CompletedDate { get; set; }

    // Trạng thái yêu cầu
    public TestRequestStatus Status { get; set; }
        = TestRequestStatus.Pending;

    // Kết luận kiểm thử
    public string? Conclusion { get; set; }

    // ==========================
    // Navigation Properties
    // ==========================

    // Phiên bản học liệu
    public virtual MaterialVersion MaterialVersion { get; set; } = null!;

    // Người kiểm thử
    public virtual User? Tester { get; set; }
}
