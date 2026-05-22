using System;

namespace Fayora.Contracts.AdminModule.GetBookings;

public record GetBookingDetailsResponse(
    Guid Id,
    Guid UserId,
    string TouristName,
    string TouristEmail,
    string TouristPhone,
    Guid ServiceProviderId,
    string ProviderName,
    string ProviderEmail,
    string ProviderPhone,
    Guid ServiceId,
    string ServiceTitle,
    string ServiceType,
    decimal BasePrice,
    decimal ServiceFee,
    decimal PayoutAmount,
    decimal TotalPrice,
    int SeatsCount,
    string BookingStatus,
    string PaymentStatus,
    DateTime StartDate,
    DateTime EndDate,
    DateTimeOffset CreatedAt
);
