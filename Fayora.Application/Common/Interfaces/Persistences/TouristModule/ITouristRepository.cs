using Fayora.Domain.Entities.TouristModule;

namespace Fayora.Application.Common.Interfaces.Persistences.TouristModule;

public interface ITouristRepository
{
    public void AddTourist(TouristProfile touristProfile);
    public Task<TouristProfile?> GetTouristByUserIdAsync(Guid userId, bool isReadOnly = true, CancellationToken cancellationToken = default!);
    public Task<bool> IsTouristProfileExistAsync(Guid userId, CancellationToken cancellationToken = default!);
    public void AddTouristInterests(IEnumerable<TouristInterest> interests);
}
