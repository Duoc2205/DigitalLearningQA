using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Common.Responses;
using MediatR;

namespace DigitalLearningQA.Application.Features.CLOs.Commands.UpdateCLO;

public class UpdateCLOHandler
    : IRequestHandler<UpdateCLOCommand, ApiResponse<Guid>>
{
    private readonly ICLORepository _repository;

    public UpdateCLOHandler(ICLORepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<Guid>> Handle(
        UpdateCLOCommand request,
        CancellationToken cancellationToken)
    {
        var clo = await _repository.GetByIdAsync(request.Id);

        if (clo == null)
        {
            return ApiResponse<Guid>.Failure("Không tìm thấy CLO.");
        }

        clo.Code = request.Code.Trim();
        clo.Description = request.Description.Trim();

        await _repository.UpdateAsync(clo);

        return ApiResponse<Guid>.SuccessResult(
            clo.Id,
            "Cập nhật CLO thành công.");
    }
}