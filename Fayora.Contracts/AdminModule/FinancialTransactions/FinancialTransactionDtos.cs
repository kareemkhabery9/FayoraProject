using System;

namespace Fayora.Contracts.AdminModule.FinancialTransactions;

public record GetTransactionsResponse(
    Guid BookingId,
    string GatewayOrderId,
    decimal Amount,
    string PaymentMethod,
    string Status,
    string? GatewayTransactionId,
    string? ErrorMessage,
    string TouristName,
    string ProviderName,
    DateTimeOffset BookingDate
);

public record GetPendingPayoutsResponse(
    Guid ProviderId,
    string ProviderName,
    string ProviderType,
    decimal TotalPendingAmount,
    int PendingBookingsCount
);
