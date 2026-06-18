using Fayora.Domain.Entities.AccommodationModule;
using Fayora.Domain.Enums.AccommodationModule;

namespace Fayora.Application.Common.Interfaces.Persistences.AccommodationModule;

public interface IHousingUnitRepository
{
    void AddUnit(HousingUnit housingUnit);
    void RemoveUnit(HousingUnit housingUnit);
    Task<HousingUnit?> GetUnitByIdAsync(Guid unitId, UnitQueryOptions? options = null, CancellationToken cancellationToken = default);

    Task<List<HousingUnit>> GetUnitsByTypeAsync(HousingType type, CancellationToken cancellationToken = default);

    Task<List<HousingUnit>> GetUnitsAsync(HousingType? type = null, string? searchTerm = null, CancellationToken cancellationToken = default);

    Task<List<HousingUnit>> GetUnitsByIdsAsync(IEnumerable<Guid> unitIds, CancellationToken cancellationToken = default);

    Task<int> GetLiveListingsStatsAsync(CancellationToken cancellationToken = default);

    Task<int> GetPendingReviewStatsAsync(CancellationToken cancellationToken = default);

    public record UnitQueryOptions(
        bool IsReadOnly = true,
        bool IncludeAmenties = false
    );
}
