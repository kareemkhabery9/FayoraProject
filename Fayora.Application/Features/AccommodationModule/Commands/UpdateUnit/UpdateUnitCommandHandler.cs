using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AccommodationModule;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Services.AuthModule;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Entities.AccommodationModule;
using Fayora.Domain.ValueObjects;

namespace Fayora.Application.Features.AccommodationModule.Commands.UpdateUnit;

public class UpdateUnitCommandHandler(
    IHousingUnitRepository housingUnitRepository,
    IHousingUnitImageRepository housingUnitImageRepository,
    IMasterAmenityRepository masterAmenityRepository,
    IClientContextProvider clientContextProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateUnitCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        var ownerId = clientContextProvider.GetContext().UserId;

        var unit = await housingUnitRepository.GetUnitByIdAsync(
            request.UnitId,
            new IHousingUnitRepository.UnitQueryOptions(IsReadOnly: false, IncludeAmenties: true),
            cancellationToken);

        if (unit is null)
            return Error.NotFound("HousingUnit.NotFound", "Housing unit not found.");

        if (unit.OwnerId != ownerId)
            return Error.Forbidden("HousingUnit.Forbidden", "You are not authorized to update this housing unit.");

        var coordinates = GeoPoint.Create(request.Latitude, request.Longitude);
        if (coordinates.IsError) return coordinates.Errors;

        var mainImageResult = FileUrl.Create(request.MainImageUrl);
        if (mainImageResult.IsError) return mainImageResult.Errors;

        var imageResults = request.ImageUrls.Select(FileUrl.Create).ToList();
        var failedImage = imageResults.FirstOrDefault(r => r.IsError);
        if (failedImage is not null) return failedImage.Errors;

        var amenities = await masterAmenityRepository.GetByIdsAsync(request.AmenityIds, cancellationToken);
        if (amenities.Count != request.AmenityIds.Count)
            return Error.Validation("HousingUnit.Amenities", "One or more of the selected amenities are invalid or inactive.");

        // Apply domain-level update
        unit.AdminUpdate(
            request.Title,
            request.Description,
            request.Type,
            request.LocationId,
            request.AddressDetails,
            coordinates.Value,
            request.NumberOfRooms,
            request.BedRooms,
            request.BathRooms,
            request.NumberOfBeds,
            request.MaxGuests,
            request.CheckInTime,
            request.CheckOutTime,
            request.PricePerNight,
            mainImageResult.Value,
            unit.Status);

        // Sync amenities: remove old, add new
        foreach (var existing in unit.Amenities.ToList())
            unit.RemoveAmenity(existing.Id);

        unit.AddAmenities(amenities);

        // Sync images: remove old, add new
        var existingImages = await housingUnitImageRepository.GetByUnitIdAsync(unit.Id, cancellationToken);
        housingUnitImageRepository.RemoveImages(existingImages);
        unit.ClearImages();

        var newImages = imageResults.Select(r => new HousingUnitImage(unit.Id, r.Value)).ToList();
        housingUnitImageRepository.AddImages(newImages);
        unit.AddImages(newImages.Select(img => img.Id));

        await unitOfWork.CommitChangesAsync(cancellationToken);

        return unit.Id;
    }
}
