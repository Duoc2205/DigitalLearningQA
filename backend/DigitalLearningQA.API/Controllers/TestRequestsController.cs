using DigitalLearningQA.Application.Features.TestRequests.Commands.CompleteTestRequest;
using DigitalLearningQA.Application.Features.TestRequests.Commands.CreateTestRequest;
using DigitalLearningQA.Application.Features.TestRequests.Commands.StartTestRequest;
using DigitalLearningQA.Application.Features.TestRequests.Queries.GetAllTestRequests;
using DigitalLearningQA.Application.Features.TestRequests.Queries.GetTestRequestById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalLearningQA.API.Controllers;

[ApiController]
[Route("api/test-requests")]
[Authorize]
public class TestRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TestRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // POST: api/test-requests
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTestRequestCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);

        return Ok(new
        {
            id,
            message = "Test request created successfully."
        });
    }

    // GET: api/test-requests
    [HttpGet]
    public async Task<ActionResult<List<TestRequestDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAllTestRequestsQuery(),
            cancellationToken);

        return Ok(result);
    }

    // GET: api/test-requests/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TestRequestDetailDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetTestRequestByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }

    // PUT: api/test-requests/{id}/start
    [HttpPut("{id:guid}/start")]
    public async Task<IActionResult> Start(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new StartTestRequestCommand(id),
            cancellationToken);

        return Ok(new
        {
            message = "Test request started successfully."
        });
    }

    // PUT: api/test-requests/{id}/complete
    [HttpPut("{id:guid}/complete")]
    public async Task<IActionResult> Complete(
        Guid id,
        [FromBody] CompleteTestRequestCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;

        await _mediator.Send(command, cancellationToken);

        return Ok(new
        {
            message = "Test request completed successfully."
        });
    }
}