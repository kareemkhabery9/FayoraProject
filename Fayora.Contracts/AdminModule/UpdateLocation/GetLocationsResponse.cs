namespace Fayora.Contracts.AdminModule.UpdateLocation;

public record GetLocationsResponse(
    int Id,
    string Name,
    string? Description,
    decimal Rating,
    decimal Latitude,
    decimal Longitude,
    string Category,
    string MainImageUrl
);
