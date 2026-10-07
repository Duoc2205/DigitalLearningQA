using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Common.Responses;
using MediatR;

namespace DigitalLearningQA.Application.Features.CLOs.Commands.DeleteCLO;

public class DeleteCLOHandler
    : IRequestHandler<DeleteCLOCommand, ApiResponse<bool>>
{
    private readonly ICLORepository _repository;

    public DeleteCLOHandler(ICLORepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<bool>> Handle(
        DeleteCLOCommand request,
        CancellationToken cancellationToken)
    {
        var clo = await _repository.GetByIdAsync(request.Id);

        if (clo == null)
        {
            return ApiResponse<bool>.Failure("Không tìm thấy CLO.");
        }

        await _repository.DeleteAsync(clo);

        return ApiResponse<bool>.SuccessResult(
            true,
            "Xóa CLO thành công.");
    }
}