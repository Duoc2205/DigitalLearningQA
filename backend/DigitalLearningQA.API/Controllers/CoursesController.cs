using DigitalLearningQA.Application.Features.Courses.Commands;
using DigitalLearningQA.Application.Features.Courses.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalLearningQA.API.Controllers;

[ApiController]
[Route("api/courses")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CoursesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllCoursesQuery());

        return Ok(result);
    }
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateCourseCommand command)
    {
    var id = await _mediator.Send(command);

    return Ok(new
    {
        Id = id,
        Message = "Đã tạo học phần thành công."
    });
}

    // Giảng viên/CCD có thể khai báo học phần thực tế khi danh mục chưa có,
    // để không bị chặn việc nộp học liệu. Dữ liệu này vẫn lưu trực tiếp vào CSDL.
    [HttpPost("self-service")]
    [Authorize(Roles = "CCD,Lecturer")]
    public async Task<IActionResult> CreateForSubmission(CreateCourseCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { Id = id, Message = "Đã thêm học phần để nộp học liệu." });
    }
}   
