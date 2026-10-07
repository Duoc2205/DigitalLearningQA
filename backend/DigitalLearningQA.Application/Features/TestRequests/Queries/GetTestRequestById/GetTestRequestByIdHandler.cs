using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.Application.Features.TestRequests.Queries.GetTestRequestById;

public class GetTestRequestByIdHandler
    : IRequestHandler<GetTestRequestByIdQuery, TestRequestDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetTestRequestByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TestRequestDetailDto> Handle(
        GetTestRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _context.TestRequests
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new TestRequestDetailDto
            {
                Id = x.Id,

                LearningMaterialId = x.MaterialVersion.LearningMaterialId,
                LearningMaterialTitle = x.MaterialVersion.LearningMaterial.Title,

                MaterialVersionId = x.MaterialVersionId,
                VersionNumber = x.MaterialVersion.VersionNumber,
                FileName = x.MaterialVersion.FileName,

                UploadedBy = x.MaterialVersion.UploadedBy,
                UploadedByName = x.MaterialVersion.UploadedUser.FullName,

                TesterId = x.TesterId,
                TesterName = x.Tester != null ? x.Tester.FullName : "AI tự động",

                RequestedDate = x.RequestedDate,
                StartedDate = x.StartedDate,
                CompletedDate = x.CompletedDate,

                Status = x.Status,
                Conclusion = x.Conclusion
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
        {
            throw new ResourceNotFoundException("Test request not found.");
        }

        return result;
    }
}
