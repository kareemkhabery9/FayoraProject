using System;
using System.Threading;
using System.Threading.Tasks;
using Fayora.Application.Common.Interfaces.Persistences.NotificationModule;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Services.SharedModule;
using Microsoft.Extensions.Logging;

namespace Fayora.Infrastructure.Jobs;

public class PushCampaignJob(
    INotificationRepository notificationRepository,
    IFirebaseNotificationService firebaseNotificationService,
    IUnitOfWork unitOfWork,
    ILogger<PushCampaignJob> logger)
{
    public async Task ExecuteAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting push campaign job execution for Campaign ID: {CampaignId}", campaignId);

        var campaign = await notificationRepository.GetPushCampaignByIdAsync(campaignId, cancellationToken);
        if (campaign == null)
        {
            logger.LogWarning("Push campaign not found for ID: {CampaignId}", campaignId);
            return;
        }

        if (campaign.Status != "Scheduled" && campaign.Status != "Draft" && campaign.Status != "Sending")
        {
            logger.LogWarning("Push campaign {CampaignId} has invalid status '{Status}' for execution. Skipping.", campaignId, campaign.Status);
            return;
        }

        try
        {
            campaign.MarkAsSending();
            await unitOfWork.CommitChangesAsync(cancellationToken);

            logger.LogInformation("Fetching device tokens for audience: {Audience}", campaign.TargetAudience);
            var tokens = await notificationRepository.GetTokensByAudienceAsync(campaign.TargetAudience, cancellationToken);

            logger.LogInformation("Broadcasting push campaign {CampaignId} to {Count} device tokens.", campaignId, tokens.Count);

            var (successCount, failureCount) = await firebaseNotificationService.SendBroadcastAsync(
                campaign.Title,
                campaign.Body,
                campaign.ImageUrl,
                tokens,
                cancellationToken);

            campaign.MarkAsSent(successCount, failureCount);
            await unitOfWork.CommitChangesAsync(cancellationToken);

            logger.LogInformation("Push campaign {CampaignId} execution completed. Success: {Success}, Failure: {Failure}", campaignId, successCount, failureCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing push campaign {CampaignId}", campaignId);
            campaign.MarkAsFailed(ex.Message);
            try
            {
                await unitOfWork.CommitChangesAsync(cancellationToken);
            }
            catch (Exception dbEx)
            {
                logger.LogError(dbEx, "Failed to update database status for failed campaign {CampaignId}", campaignId);
            }
        }
    }
}
