using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Enums.AccommodationModule;

namespace Fayora.Application.Features.AccommodationModule.Commands.CreateUnit;

public record CreateUnitCommand(
    string Title,
    string? Description,
    int LocationId,
    string AddressDetails,
    decimal Latitude,
    decimal Longitude,
    HousingType Type,
    decimal PricePerNight,
    int NumberOfRooms,
    int BedRooms,
    int BathRooms,
    int NumberOfBeds,
    int MaxGuests,
    TimeSpan CheckInTime,
    TimeSpan CheckOutTime,
    string MainImageUrl,
    string VerificationDocumentUrl,
    HashSet<string> ImageUrls,
    List<int> AmenityIds) : ICommand<Result<Guid>>;