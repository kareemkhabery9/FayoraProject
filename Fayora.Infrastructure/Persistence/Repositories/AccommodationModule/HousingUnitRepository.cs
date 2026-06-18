using Fayora.Application.Common.Interfaces.Persistences.AccommodationModule;
using Fayora.Domain.Entities.AccommodationModule;
using Fayora.Domain.Enums.AccommodationModule;
using Fayora.Domain.Enums.TourGuideModule;
using Microsoft.EntityFrameworkCore;

namespace Fayora.Infrastructure.Persistence.Repositories.AccommodationModule;

public class HousingUnitRepository(ApplicationDbContext context) : IHousingUnitRepository
{
    public void AddUnit(HousingUnit housingUnit)
    {
        context.HousingUnits.Add(housingUnit);
    }

    public void RemoveUnit(HousingUnit housingUnit)
    {
        context.HousingUnits.Remove(housingUnit);
    }

    public Task<int> GetLiveListingsStatsAsync(CancellationToken cancellationToken = default)
    {
        return context.HousingUnits.CountAsync(u => u.Status == ItemStatus.Active, cancellationToken);
    }

    public Task<int> GetPendingReviewStatsAsync(CancellationToken cancellationToken = default)
    {
        return context.HousingUnits.CountAsync(u => u.Status == ItemStatus.Pending, cancellationToken);
    }

    public async Task<HousingUnit?> GetUnitByIdAsync(
        Guid unitId,
        IHousingUnitRepository.UnitQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<HousingUnit> query = context.HousingUnits;

        if (options?.IsReadOnly == true)
            query = query.AsNoTracking();


        return await query.FirstOrDefaultAsync(u => u.Id == unitId, cancellationToken);
    }

    public async Task<List<HousingUnit>> GetUnitsByIdsAsync(
    IEnumerable<Guid> unitIds,
    CancellationToken cancellationToken = default)
    {
        if (unitIds == null || !unitIds.Any())
        {
            return [];
        }

        return await context.HousingUnits
            .AsNoTracking()
            .Where(unit => unitIds.Contains(unit.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<HousingUnit>> GetUnitsByTypeAsync(
        HousingType type,
        CancellationToken cancellationToken = default)
    {
        return await context.HousingUnits
            .AsNoTracking()
            .Where(u => u.Type == type && u.Status == ItemStatus.Active)
            .OrderBy(u => Guid.NewGuid())
            .ToListAsync(cancellationToken);
    }

    public async Task<List<HousingUnit>> GetUnitsAsync(
        HousingType? type = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.HousingUnits
            .AsNoTracking()
            .Where(u => u.Status == ItemStatus.Active);

        if (type.HasValue)
        {
            query = query.Where(u => u.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(u =>
                u.Title.ToLower().Contains(term) ||
                u.AddressDetails.ToLower().Contains(term));
        }

        return await query
            .OrderBy(u => Guid.NewGuid())
            .ToListAsync(cancellationToken);
    }
}
