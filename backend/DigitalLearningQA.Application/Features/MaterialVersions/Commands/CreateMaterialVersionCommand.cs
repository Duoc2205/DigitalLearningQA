using MediatR;

namespace DigitalLearningQA.Application.Features.MaterialVersions.Commands;

public class CreateMaterialVersionCommand : IRequest<Guid>
{
    public Guid LearningMaterialId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public Guid UploadedBy { get; set; }

    public string? Remark { get; set; }
}