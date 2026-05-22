using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;
using System;

namespace Fayora.Application.Features.AdminModule.Commands.UpdateTourPackage;

public record UpdateTourPackageCommand(
    Guid PackageId,
    string Title,
    string Description,
    int DurationHours,
    int MaxCapacity,
    decimal AdultPrice,
    decimal ChildPrice,
    string TourTypes,
    string Status) : ICommand<Result<Success>>;
