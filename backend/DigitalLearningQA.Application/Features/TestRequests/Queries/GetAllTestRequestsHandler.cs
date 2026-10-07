using DigitalLearningQA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.Application.Features.TestRequests.Queries.GetAllTestRequests;

public class GetAllTestRequestsHandler
    : IRequestHandler<GetAllTestRequestsQuery, List<TestRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllTestRequestsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TestRequestDto>> Handle(
        GetAllTestRequestsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.TestRequests
            .AsNoTracking()

            .Include(x => x.Tester)

            .Include(x => x.MaterialVersion)
                .ThenInclude(v => v.UploadedUser)

            .Include(x => x.MaterialVersion)
                .ThenInclude(v => v.LearningMaterial)

            .Select(x => new TestRequestDto
            {
                Id = x.Id,

                // Học liệu
                LearningMaterialId = x.MaterialVersion.LearningMaterialId,
                LearningMaterialTitle = x.MaterialVersion.LearningMaterial.Title,

                // Phiên bản
                MaterialVersionId = x.MaterialVersionId,
                VersionNumber = x.MaterialVersion.VersionNumber,
                FileName = x.MaterialVersion.FileName,

                // Người upload
                UploadedBy = x.MaterialVersion.UploadedBy,
                UploadedByName = x.MaterialVersion.UploadedUser.FullName,

                // Người kiểm thử
                TesterId = x.TesterId,
                TesterName = x.Tester != null ? x.Tester.FullName : "AI tự động",

                // Thời gian
                RequestedDate = x.RequestedDate,
                StartedDate = x.StartedDate,
                CompletedDate = x.CompletedDate,

                // Trạng thái
                Status = x.Status,

                // Kết luận
                Conclusion = x.Conclusion
            })
            .OrderByDescending(x => x.RequestedDate)
            .ToListAsync(cancellationToken);
    }
}
