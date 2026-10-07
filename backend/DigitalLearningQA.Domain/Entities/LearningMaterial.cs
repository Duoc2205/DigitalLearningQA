using DigitalLearningQA.Domain.Common;
using DigitalLearningQA.Domain.Enums;

namespace DigitalLearningQA.Domain.Entities;

public class LearningMaterial : BaseEntity
{
    // Mã học liệu (VD: HLS001)
    public required string Code { get; set; }

    // Môn học
    public Guid CourseId { get; set; }

    public Course Course { get; set; } = null!;

    // Giảng viên sở hữu học liệu
    public Guid OwnerId { get; set; }

    public User Owner { get; set; } = null!;

    // Tiêu đề học liệu
    public required string Title { get; set; }

    // Mô tả
    public string? Description { get; set; }

    // Loại học liệu
    public MaterialType MaterialType { get; set; }

    // Trạng thái hiện tại
    public MaterialStatus Status { get; set; } = MaterialStatus.Draft;

    // Thời điểm gửi kiểm thử
    public DateTime? SubmittedAt { get; set; }

    // Thời điểm nghiệm thu
    public DateTime? ApprovedAt { get; set; }

    // Phiên bản hiện tại
    public Guid? CurrentVersionId { get; set; }

    public MaterialVersion? CurrentVersion { get; set; }

    // Danh sách các phiên bản
    public ICollection<MaterialVersion> Versions { get; set; }
        = new List<MaterialVersion>();

    // Danh sách các chuẩn đầu ra liên quan
    public ICollection<LearningMaterialCLO> LearningMaterialCLOs
    = new List<LearningMaterialCLO>();
}