using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Features.MaterialVersions.Commands;
using DigitalLearningQA.Domain.Entities;
using MediatR;

namespace DigitalLearningQA.Application.Features.MaterialVersions.Handlers;

public class CreateMaterialVersionCommandHandler
    : IRequestHandler<CreateMaterialVersionCommand, Guid>
{
    private readonly IMaterialVersionRepository _repository;

    public CreateMaterialVersionCommandHandler(
        IMaterialVersionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(
        CreateMaterialVersionCommand request,
        CancellationToken cancellationToken)
    {
        // Lấy các version hiện có của học liệu
        var versions = await _repository.GetByMaterialIdAsync(request.LearningMaterialId);

        var nextVersion = versions.Any()
            ? versions.Max(x => x.VersionNumber) + 1
            : 1;

        var version = new MaterialVersion
        {
            LearningMaterialId = request.LearningMaterialId,
            VersionNumber = nextVersion,

            FileName = request.FileName,
            FilePath = request.FilePath,
            MimeType = request.MimeType,
            StorageProvider = "Local",
            FileSize = request.FileSize,

            UploadedBy = request.UploadedBy,
            UploadDate = DateTime.UtcNow,

            Status = DigitalLearningQA.Domain.Enums.MaterialStatus.Draft,
            SubmittedAt = null,
            ApprovedAt = null,

            Remark = request.Remark
        };

        await _repository.AddAsync(version);

        return version.Id;
    }
}