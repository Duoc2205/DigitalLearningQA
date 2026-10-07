using MediatR;

namespace DigitalLearningQA.Application.Features.MaterialVersions.Commands;

public class UploadMaterialVersionCommand : IRequest<Guid>
{
    public Guid LearningMaterialId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public Stream FileStream { get; set; } = Stream.Null;

    public Guid UploadedBy { get; set; }

    public string? Remark { get; set; }
}