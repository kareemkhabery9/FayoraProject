using FluentValidation;

namespace Fayora.Application.Features.AccommodationModule.Commands.UpdateUnit;

public class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
{
    public UpdateUnitCommandValidator()
    {
        RuleFor(x => x.UnitId)
            .NotEmpty().WithMessage("Unit ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

        RuleFor(x => x.AddressDetails)
            .NotEmpty().WithMessage("Address details are required.")
            .MaximumLength(500).WithMessage("Address details must not exceed 500 characters.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("A valid housing type must be selected.");

        RuleFor(x => x.PricePerNight)
            .GreaterThan(0).WithMessage("Price per night must be greater than zero.");

        RuleFor(x => x.NumberOfRooms)
            .GreaterThan(0).WithMessage("Number of rooms must be greater than zero.");

        RuleFor(x => x.BedRooms)
            .GreaterThanOrEqualTo(0).WithMessage("Bedrooms cannot be negative.");

        RuleFor(x => x.BathRooms)
            .GreaterThanOrEqualTo(0).WithMessage("Bathrooms cannot be negative.");

        RuleFor(x => x.NumberOfBeds)
            .GreaterThan(0).WithMessage("Number of beds must be greater than zero.");

        RuleFor(x => x.MaxGuests)
            .GreaterThan(0).WithMessage("Max guests must be greater than zero.");

        RuleFor(x => x.CheckInTime)
            .LessThan(x => x.CheckOutTime).WithMessage("Check-in time must be before check-out time.");

        RuleFor(x => x.MainImageUrl)
            .NotEmpty().WithMessage("Main image URL is required.")
            .MaximumLength(2048).WithMessage("Main image URL must not exceed 2048 characters.");

        RuleFor(x => x.ImageUrls)
            .NotEmpty().WithMessage("At least one additional image URL must be provided.");

        RuleForEach(x => x.ImageUrls)
            .NotEmpty().WithMessage("Image URL cannot be empty.")
            .MaximumLength(2048).WithMessage("Image URL must not exceed 2048 characters.");

        RuleFor(x => x.AmenityIds)
            .NotEmpty().WithMessage("At least one amenity must be selected.");

        RuleForEach(x => x.AmenityIds)
            .NotEmpty().WithMessage("Amenity ID cannot be empty.");
    }
}
