using Fayora.Domain.Common.Results;
using Fayora.Domain.Enums.AccommodationModule;
using Fayora.Domain.Enums.SharedModule;
using Fayora.Domain.Enums.TourGuideModule;
using Fayora.Domain.ValueObjects;

namespace Fayora.Domain.Entities.AccommodationModule;

public class HousingUnit : BaseEntity<Guid>
{
    public Guid OwnerId { get; init; }

    public string Title { get; private set; }
    public string? Description { get; private set; }
    public HousingType Type { get; private set; }

    public int LocationId { get; private set; }
    public string AddressDetails { get; private set; }
    public GeoPoint Coordinates { get; private set; }

    public int NumberOfRooms { get; private set; }
    public int BedRooms { get; private set; }
    public int BathRooms { get; private set; }
    public int NumberOfBeds { get; private set; }
    public int MaxGuests { get; private set; }

    public TimeSpan CheckInTime { get; private set; }
    public TimeSpan CheckOutTime { get; private set; }

    public decimal PricePerNight { get; private set; }
    public decimal CommissionRate { get; private set; }
    public CancellationPolicy CancellationPolicy { get; private set; }
    public ItemStatus Status { get; private set; }
    public decimal Rating { get; private set; }
    public int ReviewCount { get; private set; }
    public int Views { get; private set; }

    public FileUrl MainImageUrl { get; private set; } = null!;
    public FileUrl VerificationDocumentUrl { get; private set; } = null!;

    private readonly List<Guid> _imageIds = [];
    public IReadOnlyCollection<Guid> ImageIds => _imageIds.AsReadOnly();
    private readonly List<MasterAmenity> _amenities = [];
    public IReadOnlyCollection<MasterAmenity> Amenities => _amenities.AsReadOnly();

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public string? AdminNotes { get; private set; }

    private HousingUnit(
        Guid ownerId,
        string title,
        string? description,
        int locationId,
        string addressDetails,
        GeoPoint coordinates,
        HousingType type,
        decimal pricePerNight,
        int numberOfRooms,
        int bedRooms,
        int bathRooms,
        int numberOfBeds,
        int maxGuests,
        TimeSpan checkInTime,
        TimeSpan checkOutTime,
        FileUrl mainImageUrl,
        FileUrl verificationDocumentUrl,
        List<MasterAmenity> amenities)
    {
        OwnerId = ownerId;
        Title = title;
        Description = description;
        LocationId = locationId;
        AddressDetails = addressDetails;
        Coordinates = coordinates;
        Type = type;
        PricePerNight = pricePerNight;
        NumberOfRooms = numberOfRooms;
        BedRooms = bedRooms;
        BathRooms = bathRooms;
        NumberOfBeds = numberOfBeds;
        MaxGuests = maxGuests;
        CheckInTime = checkInTime;
        CheckOutTime = checkOutTime;
        MainImageUrl = mainImageUrl;
        VerificationDocumentUrl = verificationDocumentUrl;
        Status = ItemStatus.Pending;
        Rating = 0m;
        ReviewCount = 0;
        Views = 0;
        AddAmenities(amenities);
    }

    public static Result<HousingUnit> Create(
        Guid ownerId,
        string title,
        string? description,
        int locationId,
        string addressDetails,
        GeoPoint coordinates,
        HousingType type,
        decimal pricePerNight,
        int numberOfRooms,
        int bedRooms,
        int bathRooms,
        int numberOfBeds,
        int maxGuests,
        TimeSpan checkInTime,
        TimeSpan checkOutTime,
        FileUrl mainImageUrl,
        FileUrl verificationDocumentUrl,
        List<MasterAmenity> amenities)
    {
        if (ownerId == Guid.Empty)
            return Error.Validation("HousingUnit.OwnerId", "Owner ID is required.");

        if (string.IsNullOrWhiteSpace(title))
            return Error.Validation("HousingUnit.Title", "Title is required.");

        if (title.Length > 100)
            return Error.Validation("HousingUnit.Title", "Title must not exceed 100 characters.");

        if (pricePerNight <= 0)
            return Error.Validation("HousingUnit.PricePerNight", "Price per night must be greater than zero.");

        if (numberOfRooms <= 0)
            return Error.Validation("HousingUnit.NumberOfRooms", "Number of rooms must be greater than zero.");

        if (bedRooms < 0)
            return Error.Validation("HousingUnit.BedRooms", "Bedrooms cannot be negative.");

        if (bathRooms < 0)
            return Error.Validation("HousingUnit.BathRooms", "Bathrooms cannot be negative.");

        if (numberOfBeds <= 0)
            return Error.Validation("HousingUnit.NumberOfBeds", "Number of beds must be greater than zero.");

        if (maxGuests <= 0)
            return Error.Validation("HousingUnit.MaxGuests", "Max guests must be greater than zero.");

        if (type == HousingType.Villa && maxGuests != 1)
            return Error.Validation("HousingUnit.MaxGuests", "Villa type housing units must accommodate equal to 1 guest.");

        if (checkInTime >= checkOutTime)
            return Error.Validation("HousingUnit.CheckInTime", "Check-in time must be before check-out time.");

        if (string.IsNullOrWhiteSpace(addressDetails))
            return Error.Validation("HousingUnit.AddressDetails", "Address details are required.");

        amenities ??= [];

        return new HousingUnit(
            ownerId,
            title,
            description,
            locationId,
            addressDetails,
            coordinates,
            type,
            pricePerNight,
            numberOfRooms,
            bedRooms,
            bathRooms,
            numberOfBeds,
            maxGuests,
            checkInTime,
            checkOutTime,
            mainImageUrl,
            verificationDocumentUrl,
            amenities);
    }

    public Result<Success> Approve()
    {
        if (Status != ItemStatus.Pending)
            return Error.Validation("HousingUnit.Status", "Only pending housing units can be approved.");

        Status = ItemStatus.Active;
        return Result.Success;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice > 0) PricePerNight = newPrice;
    }

    public void IncrementViews() => Views++;

    public void AddImage(Guid id) => _imageIds.Add(id);

    public void AddImages(IEnumerable<Guid> ids) => _imageIds.AddRange(ids);

    public void RemoveImage(Guid imageId) => _imageIds.Remove(imageId);

    public Result<Success> Reject(string adminNotes)
    {
        if (Status != ItemStatus.Pending)
            return Error.Validation("HousingUnit.Status", "Only pending housing units can be rejected.");

        Status = ItemStatus.Rejected;
        AdminNotes = adminNotes;
        return Result.Success;
    }

    public void AddAmenity(MasterAmenity amenity)
    {
        if (!_amenities.Any(a => a.Id == amenity.Id))
        {
            _amenities.Add(amenity);
        }
    }

    public void AddAmenities(IEnumerable<MasterAmenity> amenities)
    {
        foreach (var amenity in amenities)
        {
            AddAmenity(amenity);
        }
    }

    public void RemoveAmenity(int amenityId)
    {
        var amenity = _amenities.FirstOrDefault(a => a.Id == amenityId);
        if (amenity is not null)
        {
            _amenities.Remove(amenity);
        }
    }

    public void AdminUpdate(
        string title,
        string? description,
        HousingType type,
        int locationId,
        string addressDetails,
        GeoPoint coordinates,
        int numberOfRooms,
        int bedRooms,
        int bathRooms,
        int numberOfBeds,
        int maxGuests,
        TimeSpan checkInTime,
        TimeSpan checkOutTime,
        decimal pricePerNight,
        FileUrl mainImageUrl,
        ItemStatus status)
    {
        Title = title;
        Description = description;
        Type = type;
        LocationId = locationId;
        AddressDetails = addressDetails;
        Coordinates = coordinates;
        NumberOfRooms = numberOfRooms;
        BedRooms = bedRooms;
        BathRooms = bathRooms;
        NumberOfBeds = numberOfBeds;
        MaxGuests = maxGuests;
        CheckInTime = checkInTime;
        CheckOutTime = checkOutTime;
        PricePerNight = pricePerNight;
        MainImageUrl = mainImageUrl;
        Status = status;
    }

    public record PricingResult(decimal TotalPrice, decimal ServiceFee, decimal PayoutAmount);

    public Result<PricingResult> CalculatePricing(int nights)
    {
        if (nights <= 0)
            return Error.Validation("HousingUnit.InvalidNights", "Number of nights must be greater than zero.");

        decimal totalPrice = PricePerNight * nights;
        decimal serviceFee = totalPrice * CommissionRate;
        decimal payoutAmount = totalPrice - serviceFee;

        return new PricingResult(totalPrice, serviceFee, payoutAmount);
    }

    public void AddReview(decimal newRating)
    {
        Rating = ((Rating * ReviewCount) + newRating) / (ReviewCount + 1);
        ReviewCount++;
    }

    public void UpdateReview(decimal oldRating, decimal newRating)
    {
        if (ReviewCount > 0)
        {
            Rating = ((Rating * ReviewCount) - oldRating + newRating) / ReviewCount;
        }
    }

    public void DeleteReview(decimal rating)
    {
        if (ReviewCount > 1)
        {
            Rating = ((Rating * ReviewCount) - rating) / (ReviewCount - 1);
            ReviewCount--;
        }
        else
        {
            Rating = 0;
            ReviewCount = 0;
        }
    }

    private HousingUnit() { }
}