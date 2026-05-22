using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Enums.AccommodationModule;
using Fayora.Domain.Enums.TourGuideModule;
using Fayora.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Commands.UpdateAccommodation;

public class UpdateAccommodationCommandHandler(IAdminRepository adminRepository, IUnitOfWork unitOfWork) : ICommandHandler<UpdateAccommodationCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(UpdateAccommodationCommand request, CancellationToken cancellationToken)
    {
        var accommodation = await adminRepository.GetAccommodationByIdAsync(request.AccommodationId, cancellationToken);
        if (accommodation is null)
            return Error.NotFound($"Accommodation with ID {request.AccommodationId} not found.");

        if (!Enum.TryParse<HousingType>(request.Type, true, out var typeEnum))
            return Error.Validation("Type", $"Invalid HousingType: {request.Type}");

        if (!Enum.TryParse<ItemStatus>(request.Status, true, out var statusEnum))
            return Error.Validation("Status", $"Invalid ItemStatus: {request.Status}");

        var coordinatesResult = GeoPoint.Create(request.Latitude, request.Longitude);
        if (coordinatesResult.IsError)
            return coordinatesResult.Errors;

        var mainImageUrlResult = FileUrl.Create(request.MainImageUrl);
        if (mainImageUrlResult.IsError)
            return mainImageUrlResult.Errors;

        accommodation.AdminUpdate(
            request.Title,
            request.Description,
            typeEnum,
            request.LocationId,
            request.AddressDetails,
            coordinatesResult.Value,
            request.NumberOfRooms,
            request.BedRooms,
            request.BathRooms,
            request.NumberOfBeds,
            request.MaxGuests,
            request.CheckInTime,
            request.CheckOutTime,
            request.PricePerNight,
            mainImageUrlResult.Value,
            statusEnum
        );

        await unitOfWork.CommitChangesAsync(cancellationToken);

        return Result.Success;
    }
}
