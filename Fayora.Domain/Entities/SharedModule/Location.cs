using Fayora.Domain.Common.Results;
using Fayora.Domain.Enums.SharedModule;
using Fayora.Domain.ValueObjects;

namespace Fayora.Domain.Entities.SharedModule
{
    public class Location : BaseEntity<int>
    {
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }
        public GeoPoint Coordinates { get; private set; } = null!;
        public decimal Rating { get; private set; }
        public LocationCategory Category { get; private set; }
        public int ReviewCount { get; private set; }
        public FileUrl MainImageUrl { get; private set; } = null!;

        private readonly List<Guid> _imageIds = [];
        public IReadOnlyCollection<Guid> ImageIds => _imageIds.AsReadOnly();

        private Location() { }

        private Location(
            string name,
            string? description,
            decimal rating,
            GeoPoint coordinates,
            FileUrl mainImageUrl,
            LocationCategory category)
        {
            Name = name;
            Description = description;
            Coordinates = coordinates;
            MainImageUrl = mainImageUrl;
            Category = category;
            Rating = rating;
            ReviewCount = 0;
        }

        public static Result<Location> Create(
            string name,
            string? description,
            decimal rating,
            decimal latitude,
            decimal longitude,
            LocationCategory category,
            FileUrl mainImageUrl)

        {
            if (string.IsNullOrWhiteSpace(name))
                return Error.Validation("Location.Name", "Name is required.");

            var coordinates = GeoPoint.Create(latitude, longitude);
            if (coordinates.IsError) return coordinates.Errors;

            return new Location(name, description, rating, coordinates.Value, mainImageUrl, category);
        }

        public void AddImage(Guid imageId) => _imageIds.Add(imageId);
        public void AddImages(IEnumerable<Guid> imageIds) => _imageIds.AddRange(imageIds);

        public void Update(
            string name,
            string? description,
            decimal rating,
            decimal latitude,
            decimal longitude,
            LocationCategory category,
            FileUrl mainImageUrl)
        {
            Name = name;
            Description = description;
            Rating = rating;
            var coordinatesResult = GeoPoint.Create(latitude, longitude);
            if (coordinatesResult.IsSuccess)
            {
                Coordinates = coordinatesResult.Value;
            }
            Category = category;
            MainImageUrl = mainImageUrl;
        }
    }
}
