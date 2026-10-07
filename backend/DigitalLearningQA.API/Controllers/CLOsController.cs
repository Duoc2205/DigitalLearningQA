using DigitalLearningQA.Application.Features.CLOs.Commands.CreateCLO;
using DigitalLearningQA.Application.Features.CLOs.Commands.UpdateCLO;
using DigitalLearningQA.Application.Features.CLOs.Commands.DeleteCLO;
using DigitalLearningQA.Application.Features.CLOs.Queries.GetAllCLOs;
using DigitalLearningQA.Application.Features.CLOs.Queries.GetCLOById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalLearningQA.API.Controllers;

[ApiController]
[Route("api/clos")]
[Authorize]
public class CLOsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CLOsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy toàn bộ CLO
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllCLOQuery());
        return Ok(result);
    }

    /// <summary>
    /// Lấy CLO theo Id
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(
            new GetCLOByIdQuery { Id = id });

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Tạo CLO
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateCLOCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Cập nhật CLO
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateCLOCommand command)
    {
        command.Id = id;

        var result = await _mediator.Send(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Xóa CLO
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(
            new DeleteCLOCommand { Id = id });

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}