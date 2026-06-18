using Fayora.Domain.Entities.AccommodationModule;

namespace Fayora.Application.Common.Interfaces.Persistences.AccommodationModule;

public interface IHousingUnitImageRepository
{
    void AddImages(IEnumerable<HousingUnitImage> images);
    void RemoveImages(IEnumerable<HousingUnitImage> images);
    Task<List<HousingUnitImage>> GetByUnitIdAsync(Guid unitId, CancellationToken cancellationToken = default);

}
