using FluentValidation;

namespace DigitalLearningQA.Application.Features.CLOs.Commands.CreateCLO;

public class CreateCLOValidator
    : AbstractValidator<CreateCLOCommand>
{
    public CreateCLOValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(1000);
    }
}