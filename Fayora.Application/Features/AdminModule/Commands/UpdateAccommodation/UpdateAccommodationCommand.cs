using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;
using System;

namespace Fayora.Application.Features.AdminModule.Commands.UpdateAccommodation;

public record UpdateAccommodationCommand(
    Guid AccommodationId,
    string Title,
    string? Description,
    string Type,
    int LocationId,
    string AddressDetails,
    decimal Latitude,
    decimal Longitude,
    int NumberOfRooms,
    int BedRooms,
    int BathRooms,
    int NumberOfBeds,
    int MaxGuests,
    TimeSpan CheckInTime,
    TimeSpan CheckOutTime,
    decimal PricePerNight,
    string MainImageUrl,
    string Status) : ICommand<Result<Success>>;
