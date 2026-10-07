using FluentValidation;

namespace DigitalLearningQA.Application.Features.TestRequests.Commands.CreateTestRequest;

public class CreateTestRequestValidator
    : AbstractValidator<CreateTestRequestCommand>
{
    public CreateTestRequestValidator()
    {
        RuleFor(x => x.MaterialVersionId)
            .NotEmpty()
            .WithMessage("MaterialVersionId không được để trống.");

        RuleFor(x => x.TesterId)
            .NotEmpty()
            .WithMessage("TesterId không được để trống.");
    }
}