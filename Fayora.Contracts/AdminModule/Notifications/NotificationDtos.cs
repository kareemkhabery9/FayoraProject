using System;

namespace Fayora.Contracts.AdminModule.Notifications;

public record RegisterDeviceTokenRequest(
    string Token,
    string DeviceType // "Android", "iOS", "Web"
);

public record CreatePushCampaignRequest(
    string Title,
    string Body,
    string? ImageUrl,
    string TargetAudience, // "All", "Tourist", "TourGuide", "TravelAgency"
    DateTimeOffset? ScheduledAt
);

public record PushCampaignResponse(
    Guid Id,
    string Title,
    string Body,
    string? ImageUrl,
    string TargetAudience,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset? SentAt,
    string Status,
    int SuccessCount,
    int FailureCount,
    DateTimeOffset CreatedAt
);

public record SendTestPushRequest(
    string Token
);

public record PushCampaignStatsResponse(
    int TotalCampaigns,
    int ScheduledCampaigns,
    int SentCampaigns,
    int FailedCampaigns,
    int TotalSuccessDeliveries,
    int TotalFailureDeliveries
);
