using Fayora.Domain.Common.Entity;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Enums.SharedModule;
using Fayora.Domain.Enums.TourGuideModule;
using Fayora.Domain.Errors;
using Fayora.Domain.ValueObjects;

namespace Fayora.Domain.Entities.GuideModule;

public class GuidePackage : AuditableEntity<Guid>
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public TourType TourTypes { get; private set; }
    public ProviderType ProviderType { get; private set; }
    public int DurationHours { get; private set; }
    public int NumOfDays { get; private set; }
    public int NumOfNights => NumOfDays > 1 ? NumOfDays - 1 : 0;

    public int MaxCapacity { get; private set; }
    public decimal AdultPrice { get; private set; }
    public decimal ChildPrice { get; private set; }
    public bool HasGroupDiscount { get; private set; }
    public int? GroupDiscountMinPeople { get; private set; }
    public decimal? GroupDiscountPercent { get; private set; }
    public bool IsActive { get; private set; }
    public int Views { get; private set; }
    public FileUrl MainImageUrl { get; private set; } = default!;
    public FileUrl? MainVideoUrl { get; private set; }
    public string? GuestRequirements { get; private set; }
    public CancellationPolicy CancellationPolicy { get; private set; }
    public ItemStatus PackageStatus { get; private set; }

    private readonly List<int> _includedItemIds = [];
    public IReadOnlyCollection<int> IncludedItemIds => _includedItemIds.AsReadOnly();

    private readonly List<int>? _excludedItemIds = [];
    public IReadOnlyCollection<int> ExcludedItemIds => _excludedItemIds!.AsReadOnly();
    private readonly List<PackageMeetingPoint> _meetingPoints = [];
    public IReadOnlyCollection<PackageMeetingPoint> MeetingPoints => _meetingPoints.AsReadOnly();
    public string? ArrivalNote { get; private set; }
    public TransportType TransportType { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public decimal AverageRating { get; private set; }
    public int ReviewCount { get; private set; }

    private readonly List<int> _locationIds = [];
    public IReadOnlyCollection<int> LocationIds => _locationIds.AsReadOnly();

    public string? AdminNotes { get; private set; }

    private readonly List<Guid> _imageIds = [];
    public IReadOnlyCollection<Guid> ImageIds => _imageIds.AsReadOnly();
    public IReadOnlyCollection<Guid> ImageURLs => _imageIds.ToList().AsReadOnly();

    private readonly List<Guid> _activityIds = [];
    public IReadOnlyCollection<Guid> ActivityIds => _activityIds.AsReadOnly();

    private readonly List<Guid> _nightIds = [];
    public IReadOnlyCollection<Guid> NightIds => _nightIds.AsReadOnly();

    private readonly List<PackageOccurrence> _occurrences = [];
    public IReadOnlyCollection<PackageOccurrence> Occurrences => _occurrences.AsReadOnly();

    private readonly List<OptionalActivity> _optionalActivities = [];
    public IReadOnlyCollection<OptionalActivity> OptionalActivities => _optionalActivities.AsReadOnly();



    private GuidePackage() { }

    private GuidePackage(
        Guid guideId,
        string title,
        string description,
        TourType tourTypes,
        ProviderType providerType,
        int durationHours,
        int numOfDays,
        TransportType transportType,
        int maxCapacity,
        decimal adultPrice,
        decimal childPrice,
        string? arrivalNote,
        FileUrl mainImageUrl,
        FileUrl? mainVideoUrl,
        string? guestRequirements,
        CancellationPolicy cancellationPolicy)
    {
        Id = Guid.NewGuid();
        UserId = guideId;
        Title = title;
        Description = description;
        TourTypes = tourTypes;
        ProviderType = providerType;
        DurationHours = durationHours;
        NumOfDays = numOfDays;
        TransportType = transportType;
        MaxCapacity = maxCapacity;
        AdultPrice = adultPrice;
        ChildPrice = childPrice;
        ArrivalNote = arrivalNote;
        MainImageUrl = mainImageUrl;
        MainVideoUrl = mainVideoUrl;
        GuestRequirements = guestRequirements;

        IsActive = false;
        Views = 0;
        AverageRating = 0m;
        ReviewCount = 0;

        CancellationPolicy = cancellationPolicy;
        PackageStatus = ItemStatus.Pending;
    }

    public static Result<GuidePackage> Create(
        Guid guideId, string title, string description,
        TourType tourTypes, ProviderType providerType, int durationHours, int numOfDays,
        TransportType transportType,
        int maxCapacity, decimal adultPrice, decimal childPrice,
        string? arrivalNote, FileUrl mainImageUrl,
        FileUrl? mainVideoUrl = null, string? guestRequirements = null, CancellationPolicy cancellationPolicy = CancellationPolicy.NonRefundable,
        bool hasGroupDiscount = false, int? groupDiscountMinPeople = null, decimal? groupDiscountPercent = null)
    {
        if (adultPrice <= 0)
            return Error.Validation("Package.InvalidPrice", "Adult price must be positive.");

        if (durationHours <= 0)
            return Error.Validation("Package.InvalidDuration", "Duration must be greater than zero.");

        if (numOfDays <= 0)
            return Error.Validation("Package.InvalidNumOfDays", "Number of days must be greater than zero.");

        if (maxCapacity <= 0)
            return Error.Validation("Package.InvalidCapacity", "Max capacity must be greater than zero.");

        var package = new GuidePackage(guideId, title, description, tourTypes, providerType,
            durationHours, numOfDays, transportType, maxCapacity,
            adultPrice, childPrice, arrivalNote, mainImageUrl,
            mainVideoUrl, guestRequirements, cancellationPolicy);

        var discountResult = package.SetGroupDiscount(hasGroupDiscount, groupDiscountMinPeople, groupDiscountPercent);
        if (discountResult.IsError) return discountResult.Errors;

        return package;
    }

    public void AddIncludedItem(int id) => _includedItemIds.Add(id);
    public void AddExcludedItem(int id) => _excludedItemIds!.Add(id);
    public void AddImage(Guid imageId)
    {
        _imageIds.Add(imageId);
    }

    public void AddImages(IEnumerable<Guid> imageIds)
    {
        foreach (var id in imageIds) AddImage(id);
    }

    public Result<Success> Activate()
    {
        if (IsActive)
            return Error.Conflict("Package.AlreadyActive", "Package is already active.");
        IsActive = true;
        Updated();
        return Result.Success;
    }

    public Result<Success> Deactivate()
    {
        if (!IsActive)
            return Error.Conflict("Package.AlreadyInactive", "Package is already inactive.");

        IsActive = false;
        Updated();
        return Result.Success;
    }

    public void IncrementViews() => Views++;

    public Result<Success> UpdateDetails(
        string title,
        string description,
        int durationHours,
        int numOfDays,
        decimal adultPrice,
        decimal childPrice,
        TourType tourTypes,
        int maxCapacity,
        string? arrivalNote,
        TransportType transportType,
        string? guestRequirements,
        CancellationPolicy cancellationPolicy,
        FileUrl mainImageUrl,
        FileUrl? mainVideoUrl,
        bool hasGroupDiscount = false,
        int? groupDiscountMinPeople = null,
        decimal? groupDiscountPercent = null)
    {
        if (adultPrice <= 0)
            return Error.Validation("Package.InvalidPrice", "Adult price must be positive.");

        if (durationHours <= 0)
            return Error.Validation("Package.InvalidDuration", "Duration must be greater than zero.");

        if (numOfDays <= 0)
            return Error.Validation("Package.InvalidNumOfDays", "Number of days must be greater than zero.");

        if (maxCapacity <= 0)
            return Error.Validation("Package.InvalidCapacity", "Max capacity must be greater than zero.");

        var discountResult = SetGroupDiscount(hasGroupDiscount, groupDiscountMinPeople, groupDiscountPercent);
        if (discountResult.IsError) return discountResult;

        Title = title;
        Description = description;
        DurationHours = durationHours;
        NumOfDays = numOfDays;
        AdultPrice = adultPrice;
        ChildPrice = childPrice;
        TourTypes = tourTypes;
        MaxCapacity = maxCapacity;
        ArrivalNote = arrivalNote;
        TransportType = transportType;
        GuestRequirements = guestRequirements;
        CancellationPolicy = cancellationPolicy;
        MainImageUrl = mainImageUrl;
        MainVideoUrl = mainVideoUrl;
        Updated();
        return Result.Success;
    }

    public void UpdateIncludedItems(IEnumerable<int> ids)
    {
        _includedItemIds.Clear();
        _includedItemIds.AddRange(ids);
        Updated();
    }

    public void UpdateExcludedItems(IEnumerable<int> ids)
    {
        _excludedItemIds!.Clear();
        _excludedItemIds.AddRange(ids);
        Updated();
    }

    public void UpdateLocations(IEnumerable<int> locationIds)
    {
        _locationIds.Clear();
        _locationIds.AddRange(locationIds);
        Updated();
    }

    public void UpdateImages(IEnumerable<Guid> imageIds)
    {
        _imageIds.Clear();
        _imageIds.AddRange(imageIds);
        Updated();
    }

    public void UpdateActivities(IEnumerable<Guid> activityIds)
    {
        _activityIds.Clear();
        _activityIds.AddRange(activityIds);
        Updated();
    }

    public void AddLocation(int locationId) => _locationIds.Add(locationId);
    public void AddLocations(IEnumerable<int> locationIds) => _locationIds.AddRange(locationIds);

    public void AddIncludedItems(IEnumerable<int> ids)
    {
        foreach (var id in ids) _includedItemIds.Add(id);
    }

    public void AddExcludedItems(IEnumerable<int> ids)
    {
        foreach (var id in ids) _excludedItemIds!.Add(id);
    }

    public void UpdateMeetingPoints(IEnumerable<PackageMeetingPoint> meetingPoints)
    {
        _meetingPoints.Clear();
        _meetingPoints.AddRange(meetingPoints);
        Updated();
    }

    public void AddActivities(IEnumerable<Guid> activityIds)
    {
        foreach (var id in activityIds) _activityIds.Add(id);
    }

    public void Delete() => DeletedAt = DateTimeOffset.UtcNow;

    public Result<Success> Approve()
    {
        if (PackageStatus is ItemStatus.Active)
            return Error.Validation("TourGuide.AlreadyActive", "Guide is already active.");

        PackageStatus = ItemStatus.Active;
        return Result.Success;
    }

    public Result<Success> Reject(string adminNotes)
    {
        if (PackageStatus is ItemStatus.Rejected)
            return Error.Validation("TourGuide.AlreadyRejected", "Guide is already rejected.");
        PackageStatus = ItemStatus.Rejected;
        AdminNotes = adminNotes;
        return Result.Success;
    }

    public Result<decimal> CalculateBooking(int numAdults, int numChildren, Guid selectedMeetingPointId, List<Guid>? selectedOptionalActivityIds = null)
    {
        if (numAdults < 0 || numChildren < 0)
            return Error.Validation("Package.InvalidBooking", "Number of adults and children cannot be negative.");
        if (numAdults + numChildren > MaxCapacity)
            return Error.Validation("Package.OverCapacity", "Total number of guests exceeds package capacity.");
        var total = (AdultPrice * numAdults) + (ChildPrice * numChildren);

        if (selectedOptionalActivityIds is not null && selectedOptionalActivityIds.Count > 0)
        {
            var totalGuests = numAdults + numChildren;
            foreach (var activityId in selectedOptionalActivityIds)
            {
                var activity = _optionalActivities.FirstOrDefault(a => a.Id == activityId);
                if (activity is null)
                {
                    return Error.Validation("Package.OptionalActivityNotFound", $"Optional activity with ID {activityId} not found in this package.");
                }
                total += activity.AdditionalPrice * totalGuests;
            }
        }

        var meetingPoint = _meetingPoints.FirstOrDefault(mp => mp.Id == selectedMeetingPointId);
        if (meetingPoint is null)
        {
            return Error.Validation("Package.MeetingPointNotFound", $"Meeting point with ID {selectedMeetingPointId} not found in this package.");
        }
        total += meetingPoint.Price;

        // Apply group discount if enabled and guest count meets threshold
        if (HasGroupDiscount && GroupDiscountMinPeople.HasValue && GroupDiscountPercent.HasValue)
        {
            var totalGuests = numAdults + numChildren;
            if (totalGuests >= GroupDiscountMinPeople.Value)
            {
                var discountAmount = total * (GroupDiscountPercent.Value / 100m);
                total -= discountAmount;
            }
        }

        return total;
    }

    private Result<Success> SetGroupDiscount(bool hasGroupDiscount, int? groupDiscountMinPeople, decimal? groupDiscountPercent)
    {
        if (hasGroupDiscount)
        {
            if (!groupDiscountMinPeople.HasValue || groupDiscountMinPeople.Value <= 0)
                return Error.Validation("Package.InvalidGroupDiscountMinPeople", "Group discount minimum people must be greater than zero.");

            if (!groupDiscountPercent.HasValue || groupDiscountPercent.Value <= 0 || groupDiscountPercent.Value > 100)
                return Error.Validation("Package.InvalidGroupDiscountPercent", "Group discount percent must be between 0 and 100.");
        }

        HasGroupDiscount = hasGroupDiscount;
        GroupDiscountMinPeople = hasGroupDiscount ? groupDiscountMinPeople : null;
        GroupDiscountPercent = hasGroupDiscount ? groupDiscountPercent : null;
        return Result.Success;
    }


    public Result<List<PackageOccurrence>> AddOccurrences(IEnumerable<(DateOnly Date, int AvailableSeats)> newOccurrences)
    {
        var requestedDates = newOccurrences.Select(x => x.Date).ToList();

        var hasDuplicatesInRequest = requestedDates.GroupBy(x => x).Any(g => g.Count() > 1);
        if (hasDuplicatesInRequest)
            return GuideErrors.DuplicateDatesInRequest;

        var hasOverlapWithExisting = _occurrences.Any(existing => requestedDates.Contains(existing.Date));
        if (hasOverlapWithExisting)
            return GuideErrors.PackageOccurrenceOverlap;

        var addedOccurrences = new List<PackageOccurrence>();
        foreach (var item in newOccurrences)
        {
            var occurrence = new PackageOccurrence(this.Id, item.Date, item.AvailableSeats);
            _occurrences.Add(occurrence);
            addedOccurrences.Add(occurrence);
        }

        return addedOccurrences;
    }

    public Result<Success> RemoveOccurrence(Guid occurrenceId)
    {
        var occurrence = _occurrences.FirstOrDefault(o => o.Id == occurrenceId);

        if (occurrence is null)
            return GuideErrors.OccurrenceNotFound;


        _occurrences.Remove(occurrence);
        return Result.Success;
    }

    public Result<Success> SyncOccurrences(IEnumerable<(Guid? Id, DateOnly Date, int AvailableSeats)> requestedOccurrences)
    {
        var requestedList = requestedOccurrences.ToList();

        if (requestedList.GroupBy(x => x.Date).Any(g => g.Count() > 1))
            return GuideErrors.DuplicateDatesInRequest;

        var incomingIds = requestedList.Where(x => x.Id.HasValue).Select(x => x.Id.Value).ToList();

        var occurrencesToRemove = _occurrences.Where(o => !incomingIds.Contains(o.Id)).ToList();
        foreach (var toRemove in occurrencesToRemove)
        {
            _occurrences.Remove(toRemove);
        }

        foreach (var request in requestedList)
        {
            if (request.Id.HasValue)
            {
                var existingOccurrence = _occurrences.FirstOrDefault(o => o.Id == request.Id.Value);
                if (existingOccurrence != null)
                {
                    if (existingOccurrence.Date != request.Date && _occurrences.Any(o => o.Id != existingOccurrence.Id && o.Date == request.Date))
                        return GuideErrors.PackageOccurrenceOverlap;

                    existingOccurrence.Update(request.Date, request.AvailableSeats);
                }
            }
            else
            {
                if (_occurrences.Any(o => o.Date == request.Date))
                    return GuideErrors.PackageOccurrenceOverlap;

                _occurrences.Add(new PackageOccurrence(this.Id, request.Date, request.AvailableSeats));
            }
        }

        return Result.Success;
    }

    public void AdminUpdate(
        string title,
        string description,
        int durationHours,
        int maxCapacity,
        decimal adultPrice,
        decimal childPrice,
        TourType tourTypes,
        ItemStatus status)
    {
        Title = title;
        Description = description;
        DurationHours = durationHours;
        MaxCapacity = maxCapacity;
        AdultPrice = adultPrice;
        ChildPrice = childPrice;
        TourTypes = tourTypes;
        PackageStatus = status;
        Updated();
    }

    public void AddNight(Guid nightId) => _nightIds.Add(nightId);
    public void AddNights(IEnumerable<Guid> nightIds) => _nightIds.AddRange(nightIds);

    public void UpdateNights(IEnumerable<Guid> nightIds)
    {
        _nightIds.Clear();
        _nightIds.AddRange(nightIds);
        Updated();
    }

    public void UpdateOptionalActivities(IEnumerable<OptionalActivity> optionalActivities)
    {
        _optionalActivities.Clear();
        _optionalActivities.AddRange(optionalActivities);
        Updated();
    }

    public void AddReview(decimal newRating)
    {
        AverageRating = ((AverageRating * ReviewCount) + newRating) / (ReviewCount + 1);
        ReviewCount++;
    }

    public void UpdateReview(decimal oldRating, decimal newRating)
    {
        if (ReviewCount > 0)
        {
            AverageRating = ((AverageRating * ReviewCount) - oldRating + newRating) / ReviewCount;
        }
    }

    public void DeleteReview(decimal rating)
    {
        if (ReviewCount > 1)
        {
            AverageRating = ((AverageRating * ReviewCount) - rating) / (ReviewCount - 1);
            ReviewCount--;
        }
        else
        {
            AverageRating = 0;
            ReviewCount = 0;
        }
    }
}