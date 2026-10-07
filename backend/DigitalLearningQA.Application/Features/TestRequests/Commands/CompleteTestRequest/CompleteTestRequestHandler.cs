using DigitalLearningQA.Application.Common.Exceptions;
using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.Application.Features.TestRequests.Commands.CompleteTestRequest;

public class CompleteTestRequestHandler
    : IRequestHandler<CompleteTestRequestCommand>
{
    private readonly IApplicationDbContext _context;

    public CompleteTestRequestHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        CompleteTestRequestCommand request,
        CancellationToken cancellationToken)
    {
        var testRequest = await _context.TestRequests
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (testRequest == null)
        {
            throw new ResourceNotFoundException(
                "Test request not found.");
        }

        if (testRequest.Status != TestRequestStatus.InProgress)
        {
            throw new ConflictException(
                "Only in-progress test requests can be completed.");
        }

        testRequest.Status = TestRequestStatus.Completed;
        testRequest.CompletedDate = DateTime.UtcNow;
        testRequest.Conclusion = request.Conclusion;

        await _context.SaveChangesAsync(cancellationToken);
    }
}