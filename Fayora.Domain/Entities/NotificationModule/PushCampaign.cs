using System;
using Fayora.Domain.Common.Entity;

namespace Fayora.Domain.Entities.NotificationModule;

public class PushCampaign : AuditableEntity<Guid>
{
    public string Title { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public string TargetAudience { get; private set; } = null!; // "All", "Tourist", "TourGuide", "TravelAgency"
    public DateTimeOffset? ScheduledAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public string Status { get; private set; } = null!; // "Draft", "Scheduled", "Sending", "Sent", "Failed", "Cancelled"
    public int SuccessCount { get; private set; }
    public int FailureCount { get; private set; }
    public Guid CreatedByAdminId { get; private set; }
    public string? HangfireJobId { get; private set; }

    private PushCampaign() { }

    public static PushCampaign Create(
        string title,
        string body,
        string? imageUrl,
        string targetAudience,
        DateTimeOffset? scheduledAt,
        Guid createdByAdminId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body cannot be null or empty.", nameof(body));

        var status = scheduledAt.HasValue && scheduledAt.Value > DateTimeOffset.UtcNow 
            ? "Scheduled" 
            : "Draft"; // Will be "Sending" shortly if sent immediately

        return new PushCampaign
        {
            Id = Guid.NewGuid(),
            Title = title,
            Body = body,
            ImageUrl = imageUrl,
            TargetAudience = targetAudience,
            ScheduledAt = scheduledAt,
            Status = status,
            CreatedByAdminId = createdByAdminId
        };
    }

    public void MarkAsSending()
    {
        Status = "Sending";
        Updated();
    }

    public void MarkAsSent(int successCount, int failureCount)
    {
        Status = "Sent";
        SentAt = DateTimeOffset.UtcNow;
        SuccessCount = successCount;
        FailureCount = failureCount;
        Updated();
    }

    public void MarkAsFailed(string errorDetails)
    {
        Status = "Failed";
        Updated();
    }

    public void Cancel()
    {
        if (Status == "Scheduled")
        {
            Status = "Cancelled";
            Updated();
        }
    }

    public void UpdateJobId(string jobId)
    {
        HangfireJobId = jobId;
        Updated();
    }
}
