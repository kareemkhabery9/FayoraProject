using Fayora.Application.Common.Interfaces.Persistences.AccommodationModule;
using Fayora.Domain.Entities.AccommodationModule;
using Microsoft.EntityFrameworkCore;

namespace Fayora.Infrastructure.Persistence.Repositories.AccommodationModule;

public class HousingUnitImageRepository(ApplicationDbContext context) : IHousingUnitImageRepository
{
    public void AddImages(IEnumerable<HousingUnitImage> images)
    {
        context.HousingUnitImages.AddRange(images);
    }

    public void RemoveImages(IEnumerable<HousingUnitImage> images)
    {
        context.HousingUnitImages.RemoveRange(images);
    }

    public async Task<List<HousingUnitImage>> GetByUnitIdAsync(
        Guid unitId,
        CancellationToken cancellationToken = default)
    {
        return await context.HousingUnitImages
            .AsNoTracking()
            .Where(i => i.UnitId == unitId)
            .ToListAsync(cancellationToken);
    }
}
