using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AccommodationModule;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Services.AuthModule;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AccommodationModule.Commands.DeleteUnit;

public class DeleteUnitCommandHandler(
    IHousingUnitRepository housingUnitRepository,
    IHousingUnitImageRepository housingUnitImageRepository,
    IClientContextProvider clientContextProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteUnitCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
    {
        var ownerId = clientContextProvider.GetContext().UserId;

        var unit = await housingUnitRepository.GetUnitByIdAsync(
            request.UnitId,
            new IHousingUnitRepository.UnitQueryOptions(IsReadOnly: false),
            cancellationToken);

        if (unit is null)
            return Error.NotFound("HousingUnit.NotFound", "Housing unit not found.");

        if (unit.OwnerId != ownerId)
            return Error.Forbidden("HousingUnit.Forbidden", "You are not authorized to delete this housing unit.");

        // Remove related images first
        var images = await housingUnitImageRepository.GetByUnitIdAsync(unit.Id, cancellationToken);
        housingUnitImageRepository.RemoveImages(images);

        housingUnitRepository.RemoveUnit(unit);

        await unitOfWork.CommitChangesAsync(cancellationToken);

        return Result.Success;
    }
}
