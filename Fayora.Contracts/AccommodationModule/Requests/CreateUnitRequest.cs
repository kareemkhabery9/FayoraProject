namespace Fayora.Contracts.AccommodationModule.Requests;

public record CreateUnitRequest(
    string Title,
    string? Description,
    int LocationId,
    string AddressDetails,
    decimal Latitude,
    decimal Longitude,
    string Type,
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
    HashSet<int> AmenityIds
);