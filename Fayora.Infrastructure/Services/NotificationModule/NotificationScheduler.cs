using System;
using System.Threading;
using Fayora.Application.Common.Interfaces.Services.NotificationModule;
using Fayora.Infrastructure.Jobs;
using Hangfire;

namespace Fayora.Infrastructure.Services.NotificationModule;

public class NotificationScheduler(IBackgroundJobClient backgroundJobClient) : INotificationScheduler
{
    public string ScheduleCampaign(Guid campaignId, DateTimeOffset scheduledAt)
    {
        return backgroundJobClient.Schedule<PushCampaignJob>(
            job => job.ExecuteAsync(campaignId, CancellationToken.None),
            scheduledAt);
    }

    public string EnqueueCampaign(Guid campaignId)
    {
        return backgroundJobClient.Enqueue<PushCampaignJob>(
            job => job.ExecuteAsync(campaignId, CancellationToken.None));
    }

    public bool CancelCampaign(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return false;
        }
        return backgroundJobClient.Delete(jobId);
    }
}
