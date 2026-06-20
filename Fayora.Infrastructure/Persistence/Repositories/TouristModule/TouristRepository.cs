using Fayora.Application.Common.Interfaces.Persistences.TouristModule;
using Fayora.Domain.Entities.TouristModule;
using Microsoft.EntityFrameworkCore;

namespace Fayora.Infrastructure.Persistence.Repositories.TouristModule;

public class TouristRepository(ApplicationDbContext context) : ITouristRepository
{
    public void AddTourist(TouristProfile touristProfile)
    {
        context.Tourists.Add(touristProfile);
    }

    public Task<TouristProfile?> GetTouristByUserIdAsync(Guid userId, bool isReadOnly = true, CancellationToken cancellationToken = default!)
    {
        if (isReadOnly)
        {
            return context.Tourists.AsNoTracking().FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);
        }
        else
        {
            return context.Tourists.FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);
        }
    }

    public async Task<bool> IsTouristProfileExistAsync(Guid userId, CancellationToken cancellationToken = default!)
    {
        return await context.Tourists.AnyAsync(t => t.UserId == userId, cancellationToken);
    }

    public void AddTouristInterests(IEnumerable<TouristInterest> interests)
    {
        context.TouristInterests.AddRange(interests);
    }
}
