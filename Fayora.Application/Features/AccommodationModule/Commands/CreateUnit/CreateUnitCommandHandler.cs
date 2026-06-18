using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AccommodationModule;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Services.AuthModule;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Entities.AccommodationModule;
using Fayora.Domain.ValueObjects;

namespace Fayora.Application.Features.AccommodationModule.Commands.CreateUnit;

public class CreateUnitCommandHandler(
    IMasterAmenityRepository masterAmenityRepository,
    IClientContextProvider clientContextProvider,
    IHousingUnitRepository housingUnitRepository,
    IHousingUnitImageRepository housingUnitImageRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateUnitCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        var ownerId = clientContextProvider.GetContext().UserId;

        var coordinates = GeoPoint.Create(request.Latitude, request.Longitude);
        if (coordinates.IsError) return coordinates.Errors;

        var mainImageResult = FileUrl.Create(request.MainImageUrl);
        if (mainImageResult.IsError) return mainImageResult.Errors;

        var verificationDocumentResult = FileUrl.Create(request.VerificationDocumentUrl);
        if (verificationDocumentResult.IsError) return verificationDocumentResult.Errors;

        var imageResults = request.ImageUrls.Select(FileUrl.Create).ToList();
        var failedImage = imageResults.FirstOrDefault(r => r.IsError);
        if (failedImage is not null) return failedImage.Errors;

        var amenities = await masterAmenityRepository.GetByIdsAsync(request.AmenityIds, cancellationToken);

        if (amenities.Count != request.AmenityIds.Count)
        {
            return Error.Validation("HousingUnit.Amenities", "One or more of the selected amenities are invalid or inactive.");
        }

        var housingUnitResult = HousingUnit.Create(
            ownerId,
            request.Title,
            request.Description,
            request.LocationId,
            request.AddressDetails,
            coordinates.Value,
            request.Type,
            request.PricePerNight,
            request.NumberOfRooms,
            request.BedRooms,
            request.BathRooms,
            request.NumberOfBeds,
            request.MaxGuests,
            request.CheckInTime,
            request.CheckOutTime,
            mainImageResult.Value,
            verificationDocumentResult.Value,
            amenities);

        if (housingUnitResult.IsError) return housingUnitResult.Errors;
        var housingUnit = housingUnitResult.Value;

        var unitImages = imageResults.Select(r => new HousingUnitImage(housingUnit.Id, r.Value)).ToList();

        housingUnit.AddImages(unitImages.Select(image => image.Id));

        housingUnitImageRepository.AddImages(unitImages);

        housingUnitRepository.AddUnit(housingUnit);

        await unitOfWork.CommitChangesAsync(cancellationToken);

        return housingUnit.Id;
    }
}