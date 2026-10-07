namespace DigitalLearningQA.Application.Features.MaterialVersions.Queries;

public class MaterialVersionDto
{
    public Guid Id { get; set; }

    public int VersionNumber { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadDate { get; set; }

    public string UploadedBy { get; set; } = string.Empty;

    public string? Remark { get; set; }

    public int Status { get; set; }
}