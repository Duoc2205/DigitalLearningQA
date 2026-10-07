using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Features.MaterialVersions.Queries;
using MediatR;

namespace DigitalLearningQA.Application.Features.MaterialVersions.Handlers;

public class GetMaterialVersionsQueryHandler
    : IRequestHandler<GetMaterialVersionsQuery, List<MaterialVersionDto>>
{
    private readonly IMaterialVersionRepository _repository;

    public GetMaterialVersionsQueryHandler(
        IMaterialVersionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MaterialVersionDto>> Handle(
        GetMaterialVersionsQuery request,
        CancellationToken cancellationToken)
    {
        var versions =
            await _repository.GetByMaterialIdAsync(request.LearningMaterialId);

        return versions.Select(x => new MaterialVersionDto
        {
            Id = x.Id,
            VersionNumber = x.VersionNumber,
            FileName = x.FileName,
            MimeType = x.MimeType,
            FileSize = x.FileSize,
            UploadDate = x.UploadDate,
            UploadedBy = x.UploadedUser.FullName,
            Remark = x.Remark,
            Status = (int)x.Status
        }).ToList();
    }
}