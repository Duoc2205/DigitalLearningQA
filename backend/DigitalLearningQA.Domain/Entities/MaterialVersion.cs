using DigitalLearningQA.Domain.Common;
using DigitalLearningQA.Domain.Enums;

namespace DigitalLearningQA.Domain.Entities;

public class MaterialVersion : BaseEntity
{
    public Guid LearningMaterialId { get; set; }

    public LearningMaterial LearningMaterial { get; set; } = null!;

    public int VersionNumber { get; set; }

    public required string FileName { get; set; }

    public required string FilePath { get; set; }

    public required string MimeType { get; set; }

    public string StorageProvider { get; set; } = "Local";

    public long FileSize { get; set; }

    public Guid UploadedBy { get; set; }

    public User UploadedUser { get; set; } = null!;

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public MaterialStatus Status { get; set; } = MaterialStatus.Draft;

    public DateTime? SubmittedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? Remark { get; set; }

    
    public ICollection<TestRequest> TestRequests { get; set; }
        = new List<TestRequest>();
}