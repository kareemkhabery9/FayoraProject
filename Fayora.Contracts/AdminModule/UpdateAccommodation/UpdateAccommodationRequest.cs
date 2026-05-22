using System;

namespace Fayora.Contracts.AdminModule.UpdateAccommodation;

public record UpdateAccommodationRequest(
    string Title,
    string? Description,
    string Type,
    int LocationId,
    string AddressDetails,
    decimal Latitude,
    decimal Longitude,
    int NumberOfRooms,
    int BedRooms,
    int BathRooms,
    int NumberOfBeds,
    int MaxGuests,
    TimeSpan CheckInTime,
    TimeSpan CheckOutTime,
    decimal PricePerNight,
    string MainImageUrl,
    string Status
);
