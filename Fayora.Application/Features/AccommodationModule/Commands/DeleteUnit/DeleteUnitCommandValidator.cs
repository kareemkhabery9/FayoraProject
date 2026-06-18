using FluentValidation;

namespace Fayora.Application.Features.AccommodationModule.Commands.DeleteUnit;

public class DeleteUnitCommandValidator : AbstractValidator<DeleteUnitCommand>
{
    public DeleteUnitCommandValidator()
    {
        RuleFor(x => x.UnitId)
            .NotEmpty().WithMessage("Unit ID is required.");
    }
}
