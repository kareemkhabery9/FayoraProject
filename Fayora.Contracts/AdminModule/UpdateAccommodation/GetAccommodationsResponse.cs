using System;

namespace Fayora.Contracts.AdminModule.UpdateAccommodation;

public record GetAccommodationsResponse(
    Guid Id,
    string Title,
    Guid OwnerId,
    string OwnerFullName,
    string Type,
    decimal PricePerNight,
    string Status,
    string AddressDetails,
    DateTimeOffset CreatedAt
);
