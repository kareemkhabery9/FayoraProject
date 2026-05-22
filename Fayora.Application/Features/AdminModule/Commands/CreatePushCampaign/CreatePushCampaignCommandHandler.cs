using System;
using System.Threading;
using System.Threading.Tasks;
using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.NotificationModule;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Services.AuthModule;
using Fayora.Application.Common.Interfaces.Services.NotificationModule;
using Fayora.Domain.Common.Results;
using Fayora.Domain.Entities.NotificationModule;

namespace Fayora.Application.Features.AdminModule.Commands.CreatePushCampaign;

public class CreatePushCampaignCommandHandler(
    INotificationRepository notificationRepository,
    INotificationScheduler notificationScheduler,
    IUnitOfWork unitOfWork,
    IClientContextProvider clientContextProvider) : ICommandHandler<CreatePushCampaignCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePushCampaignCommand request, CancellationToken cancellationToken)
    {
        var clientContext = clientContextProvider.GetContext();
        if (clientContext == null || clientContext.UserId == Guid.Empty)
        {
            return Error.Unauthorized("Admin.Unauthorized", "Admin context is required to create a push campaign.");
        }

        try
        {
            var campaign = PushCampaign.Create(
                request.Title,
                request.Body,
                request.ImageUrl,
                request.TargetAudience,
                request.ScheduledAt,
                clientContext.UserId);

            await notificationRepository.AddPushCampaignAsync(campaign, cancellationToken);

            string jobId;
            if (request.ScheduledAt.HasValue && request.ScheduledAt.Value > DateTimeOffset.UtcNow)
            {
                jobId = notificationScheduler.ScheduleCampaign(campaign.Id, request.ScheduledAt.Value);
            }
            else
            {
                jobId = notificationScheduler.EnqueueCampaign(campaign.Id);
            }

            campaign.UpdateJobId(jobId);

            await unitOfWork.CommitChangesAsync(cancellationToken);

            return campaign.Id;
        }
        catch (ArgumentException ex)
        {
            return Error.Validation("Campaign.ValidationError", ex.Message);
        }
    }
}
