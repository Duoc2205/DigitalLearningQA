using Microsoft.AspNetCore.Http;

namespace DigitalLearningQA.API.DTOs.MaterialVersions;

public class UploadMaterialVersionRequest
{
    public Guid LearningMaterialId { get; set; }

    public string? Remark { get; set; }

    public IFormFile File { get; set; } = null!;
}