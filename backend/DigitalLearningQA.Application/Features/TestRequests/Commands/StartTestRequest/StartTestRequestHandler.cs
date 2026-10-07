using DigitalLearningQA.Application.Common.Exceptions;
using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.Application.Features.TestRequests.Commands.StartTestRequest;

public class StartTestRequestHandler
    : IRequestHandler<StartTestRequestCommand>
{
    private readonly IApplicationDbContext _context;

    public StartTestRequestHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        StartTestRequestCommand request,
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

        if (testRequest.Status != TestRequestStatus.Pending)
        {
            throw new ConflictException(
                "Only pending test requests can be started.");
        }

        testRequest.Status = TestRequestStatus.InProgress;
        testRequest.StartedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}