namespace Fayora.Contracts.AdminModule.UpdateTourPackage;

public record UpdateTourPackageRequest(
    string Title,
    string Description,
    int DurationHours,
    int MaxCapacity,
    decimal AdultPrice,
    decimal ChildPrice,
    string TourTypes,
    string Status
);
