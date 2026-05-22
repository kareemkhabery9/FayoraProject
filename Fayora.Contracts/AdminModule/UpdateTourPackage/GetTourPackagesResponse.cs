using System;

namespace Fayora.Contracts.AdminModule.UpdateTourPackage;

public record GetTourPackagesResponse(
    Guid Id,
    string Title,
    Guid UserId,
    string CreatorFullName,
    int DurationHours,
    int MaxCapacity,
    decimal AdultPrice,
    decimal ChildPrice,
    string Status,
    DateTimeOffset CreatedAt
);
