using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Enums.SharedModule;
using Fayora.Domain.Enums.TourGuideModule;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Commands.UpdateTourPackage;

public class UpdateTourPackageCommandHandler(IAdminRepository adminRepository, IUnitOfWork unitOfWork) : ICommandHandler<UpdateTourPackageCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(UpdateTourPackageCommand request, CancellationToken cancellationToken)
    {
        var package = await adminRepository.GetTourPackageByIdAsync(request.PackageId, cancellationToken);
        if (package is null)
            return Error.NotFound($"Tour Package with ID {request.PackageId} not found.");

        if (!Enum.TryParse<TourType>(request.TourTypes, true, out var tourTypeEnum))
            return Error.Validation("TourTypes", $"Invalid TourType flag expression: {request.TourTypes}");

        if (!Enum.TryParse<ItemStatus>(request.Status, true, out var statusEnum))
            return Error.Validation("Status", $"Invalid ItemStatus: {request.Status}");

        package.AdminUpdate(
            request.Title,
            request.Description,
            request.DurationHours,
            request.MaxCapacity,
            request.AdultPrice,
            request.ChildPrice,
            tourTypeEnum,
            statusEnum
        );

        await unitOfWork.CommitChangesAsync(cancellationToken);

        return Result.Success;
    }
}
