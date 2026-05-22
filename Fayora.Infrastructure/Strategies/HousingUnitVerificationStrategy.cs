using Fayora.Application.Common.Interfaces.Persistences.AccommodationModule;
using Fayora.Application.Common.Strategies;
using Fayora.Domain.Common.Results;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Fayora.Application.Common.Interfaces.Persistences.AccommodationModule.IHousingUnitRepository;

namespace Fayora.Infrastructure.Strategies;

public class HousingUnitVerificationStrategy(IHousingUnitRepository housingUnitRepository) : IVerificationStrategy
{
    public bool CanHandle(string entityType) =>
        entityType.Equals("Accommodation", StringComparison.OrdinalIgnoreCase) ||
        entityType.Equals("HousingUnit", StringComparison.OrdinalIgnoreCase);

    public async Task<Result<Success>> ProcessVerificationAsync(Guid entityId, bool isApproved, string adminNotes, CancellationToken ct)
    {
        var housingUnit = await housingUnitRepository.GetUnitByIdAsync(entityId, new UnitQueryOptions(IsReadOnly: false), ct);

        if (housingUnit is null)
            return Error.NotFound($"Housing unit with ID {entityId} not found.");

        if (isApproved)
        {
            var approvalResult = housingUnit.Approve();
            if (approvalResult.IsError)
                return approvalResult;
        }
        else
        {
            var rejectionResult = housingUnit.Reject(adminNotes);
            if (rejectionResult.IsError)
                return rejectionResult;
        }

        return Result.Success;
    }
}
