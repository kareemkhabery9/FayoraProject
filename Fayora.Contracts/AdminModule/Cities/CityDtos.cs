namespace Fayora.Contracts.AdminModule.Cities;

public record GetCitiesResponse(
    int Id,
    string Name,
    string CountryCode,
    decimal Latitude,
    decimal Longitude
);

public record CreateCityRequest(
    string Name,
    string CountryCode,
    decimal Latitude,
    decimal Longitude
);

public record UpdateCityRequest(
    string Name,
    string CountryCode,
    decimal Latitude,
    decimal Longitude
);
