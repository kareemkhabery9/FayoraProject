using FluentValidation;

namespace Fayora.Application.Features.TouristModule.Commands.CreateTouristProfile;

public class CreateTouristProfileCommandValidator : AbstractValidator<CreateTouristProfileCommand>
{
    public CreateTouristProfileCommandValidator()
    {
        RuleFor(x => x.BudgetTier)
            .IsInEnum()
            .When(x => x.BudgetTier.HasValue)
            .WithMessage("Invalid Budget Tier value.");

        RuleFor(x => x.TravelStyle)
            .IsInEnum()
            .When(x => x.TravelStyle.HasValue)
            .WithMessage("Invalid Travel Style value.");

        RuleFor(x => x.InterestIds)
            .NotNull()
            .WithMessage("Interests list cannot be null.");

        RuleFor(x => x.InterestIds)
            .Must(interests => interests.Count <= 10)
            .When(x => x.InterestIds != null)
            .WithMessage("You can select a maximum of 10 interests only.");

        RuleForEach(x => x.InterestIds)
            .GreaterThan(0)
            .WithMessage("Interest ID must be greater than 0.");

        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required.")
            .MaximumLength(100).WithMessage("Device ID must not exceed 100 characters.");
    }
}