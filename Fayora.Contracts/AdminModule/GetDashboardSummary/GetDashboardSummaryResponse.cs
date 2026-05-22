using System;
using System.Collections.Generic;

namespace Fayora.Contracts.AdminModule.GetDashboardSummary;

public record GetDashboardSummaryResponse(
    UserStatsDto UserStats,
    BookingStatsDto BookingStats,
    ContentStatsDto ContentStats,
    List<RecentActivityDto> RecentActivities,
    ChatbotSnapshotDto ChatbotSnapshot,
    FinancialSnapshotDto FinancialSnapshot,
    GrowthIndicatorsDto GrowthIndicators
);

public record UserStatsDto(
    int TotalUsers,
    int ActiveUsers,
    int BannedUsers,
    int LockedUsers,
    int TouristCount,
    int GuideCount,
    int CompanyCount,
    int OwnerCount
);

public record BookingStatsDto(
    int TotalBookings,
    int CompletedBookings,
    int PendingBookings,
    int CancelledBookings,
    decimal TotalGmv,
    decimal TotalCommission
);

public record ContentStatsDto(
    int TotalAccommodations,
    int ActiveAccommodations,
    int PendingAccommodations,
    int TotalPackages,
    int ActivePackages,
    int PendingPackages,
    int PendingVerificationQueue
);

public record RecentActivityDto(
    string ActivityType,
    string Description,
    string TargetId,
    DateTimeOffset Timestamp
);

public record ChatbotSnapshotDto(
    int TotalSessions,
    int TotalMessages,
    int SessionsToday,
    int MessagesToday
);

public record FinancialSnapshotDto(
    decimal MonthlyRevenue,
    decimal MonthlyCommission,
    decimal PendingPayoutsTotal,
    int RefundedCount
);

public record GrowthIndicatorsDto(
    double UserGrowthPercent,
    double BookingGrowthPercent,
    int NewUsersThisMonth,
    int NewUsersLastMonth,
    int BookingsThisMonth,
    int BookingsLastMonth
);
