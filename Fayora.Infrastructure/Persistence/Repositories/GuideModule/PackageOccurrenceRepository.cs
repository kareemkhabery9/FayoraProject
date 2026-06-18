using Fayora.Application.Common.Interfaces.Persistences.GuideModule;
using Fayora.Application.Features.AdminModule.Queries.GetCalendarBookings;
using Fayora.Domain.Entities.GuideModule;
using Fayora.Domain.Enums.TourGuideModule;
using Microsoft.EntityFrameworkCore;

namespace Fayora.Infrastructure.Persistence.Repositories.GuideModule;

public class PackageOccurrenceRepository(ApplicationDbContext context)
    : IPackageOccurrenceRepository
{
    public void Add(PackageOccurrence occurrence)
    {
        context.PackageOccurrences.Add(occurrence);
    }

    public async Task AddRangeAsync(List<PackageOccurrence> occurrences, CancellationToken cancellationToken)
    {
        await context.PackageOccurrences.AddRangeAsync(occurrences, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<CalendarBookingItemDto>> GetCalendarPackagessAsync(
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var query = from occurrence in context.PackageOccurrences
                    join package in context.GuideTourPackages
                    on occurrence.PackageId equals package.Id
                    where occurrence.Date.Year == year && occurrence.Date.Month == month
                    select new CalendarBookingItemDto
                    (
                        occurrence.Id,
                        package.Title,
                        occurrence.Date,
                        "Tour"
                    );

        return await query.ToListAsync(cancellationToken);
    }

    public Task<PackageOccurrence?> GetOccurrenceByIdAsync(Guid occurrenceId, CancellationToken cancellationToken)
    {
        return context.PackageOccurrences
            .FirstOrDefaultAsync(x => x.Id == occurrenceId, cancellationToken);
    }

    public Task<PackageOccurrence?> GetOccurrenceByPackageIdAndDate(Guid packageId, DateOnly date, CancellationToken cancellationToken)
    {
        return context.PackageOccurrences
            .FirstOrDefaultAsync(x => x.PackageId == packageId && x.Date == date, cancellationToken);
    }

    public Task<List<PackageOccurrence>> GetOccurrencesByPackageIdAsync(Guid packageId, CancellationToken cancellationToken)
    {
        return context.PackageOccurrences
            .Where(x => x.PackageId == packageId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasOverlappingOccurrenceAsync(Guid packageId, List<DateOnly> dates, CancellationToken cancellationToken)
    {
        return await context.PackageOccurrences
            .AnyAsync(x => x.PackageId == packageId
                        && dates.Contains(x.Date)
                        && x.Status != OccurrenceStatus.Cancelled,
                      cancellationToken);
    }

    public async Task ReleaseSeatsAsync(
        Guid packageId,
        DateOnly date,
        int count,
        CancellationToken cancellationToken)
    {
        var occurrence = await context.PackageOccurrences
            .FirstOrDefaultAsync(x => x.PackageId == packageId
                                 && x.Date == date,
                                 cancellationToken);

        occurrence?.ReleaseSeats(count);
    }

    public void Remove(PackageOccurrence occurrence)
    {
        context.PackageOccurrences.Remove(occurrence);
    }
}
