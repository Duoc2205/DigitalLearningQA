using DigitalLearningQA.Application.Common.Exceptions;
using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Domain.Entities;
using DigitalLearningQA.Domain.Enums;
using MediatR;

namespace DigitalLearningQA.Application.Features.TestRequests.Commands.CreateTestRequest;

public class CreateTestRequestHandler
    : IRequestHandler<CreateTestRequestCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateTestRequestHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        CreateTestRequestCommand request,
        CancellationToken cancellationToken)
    {
        var materialVersion = await _context.MaterialVersions.FindAsync(
            new object[] { request.MaterialVersionId },
            cancellationToken);

        if (materialVersion == null)
        {
            throw new ResourceNotFoundException(
                "Material version not found.");
        }

        var testRequest = new TestRequest
        {
            Id = Guid.NewGuid(),
            MaterialVersionId = request.MaterialVersionId,
            TesterId = request.TesterId,
            RequestedDate = DateTime.UtcNow,
            Status = TestRequestStatus.Pending
        };

        _context.TestRequests.Add(testRequest);

        await _context.SaveChangesAsync(cancellationToken);

        return testRequest.Id;
    }
}