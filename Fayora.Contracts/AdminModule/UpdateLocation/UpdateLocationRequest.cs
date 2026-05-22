using System.Collections.Generic;

namespace Fayora.Contracts.AdminModule.UpdateLocation;

public record UpdateLocationRequest(
    string Name,
    string? Description,
    decimal Rating,
    decimal Latitude,
    decimal Longitude,
    int Category,
    string MainImageUrl,
    List<string> ImageUrls
);
