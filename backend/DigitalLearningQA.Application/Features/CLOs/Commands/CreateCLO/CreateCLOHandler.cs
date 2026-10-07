using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Common.Responses;
using DigitalLearningQA.Domain.Entities;
using MediatR;

namespace DigitalLearningQA.Application.Features.CLOs.Commands.CreateCLO;

public class CreateCLOHandler
    : IRequestHandler<CreateCLOCommand, ApiResponse<Guid>>
{
    private readonly ICLORepository _cloRepository;

    public CreateCLOHandler(
        ICLORepository cloRepository)
    {
        _cloRepository = cloRepository;
    }

    public async Task<ApiResponse<Guid>> Handle(
        CreateCLOCommand request,
        CancellationToken cancellationToken)
    {
        // Kiểm tra trùng mã CLO trong cùng học phần
        if (await _cloRepository.ExistsAsync(
            request.CourseId,
            request.Code.Trim()))
        {
            return ApiResponse<Guid>.Failure(
                "Mã CLO đã tồn tại trong học phần.");
        }

        var clo = new CLO
        {
            CourseId = request.CourseId,
            Code = request.Code.Trim(),
            Description = request.Description.Trim()
        };

        await _cloRepository.AddAsync(clo);

        return ApiResponse<Guid>.SuccessResult(
            clo.Id,
            "Tạo CLO thành công.");
    }
}