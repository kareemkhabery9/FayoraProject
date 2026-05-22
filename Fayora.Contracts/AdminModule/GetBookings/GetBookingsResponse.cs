using System;

namespace Fayora.Contracts.AdminModule.GetBookings;

public record GetBookingsResponse(
    Guid Id,
    Guid UserId,
    string UserFullName,
    Guid ServiceId,
    string ServiceType,
    decimal TotalPrice,
    string BookingStatus,
    string PaymentStatus,
    DateTime StartDate,
    DateTime EndDate,
    DateTimeOffset CreatedAt
);
