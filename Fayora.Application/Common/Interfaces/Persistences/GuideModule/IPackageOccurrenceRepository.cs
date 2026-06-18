using Fayora.Application.Features.AdminModule.Queries.GetCalendarBookings;
using Fayora.Domain.Entities.GuideModule;

namespace Fayora.Application.Common.Interfaces.Persistences.GuideModule;

public interface IPackageOccurrenceRepository
{
    void Add(PackageOccurrence occurrence);

    Task<PackageOccurrence?> GetOccurrenceByIdAsync(Guid occurrenceId, CancellationToken cancellationToken);

    Task<PackageOccurrence?> GetOccurrenceByPackageIdAndDate(Guid packageId, DateOnly date, CancellationToken cancellationToken);

    Task<List<PackageOccurrence>> GetOccurrencesByPackageIdAsync(Guid packageId, CancellationToken cancellationToken);

    Task ReleaseSeatsAsync(Guid packageId, DateOnly date, int count, CancellationToken cancellationToken);

    Task<List<CalendarBookingItemDto>> GetCalendarPackagessAsync(int year, int month, CancellationToken cancellationToken);

    void Remove(PackageOccurrence occurrence);
}
