using System.Threading;
using System.Threading.Tasks;
using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.NotificationModule;
using Fayora.Application.Common.Interfaces.Persistences.IdentityModule;
using Fayora.Application.Common.Interfaces.Services.NotificationModule;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AdminModule.Commands.CancelPushCampaign;

public class CancelPushCampaignCommandHandler(
    INotificationRepository notificationRepository,
    INotificationScheduler notificationScheduler,
    IUnitOfWork unitOfWork) : ICommandHandler<CancelPushCampaignCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(CancelPushCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await notificationRepository.GetPushCampaignByIdAsync(request.Id, cancellationToken);
        if (campaign == null)
        {
            return Error.NotFound("Campaign.NotFound", $"Push campaign with ID {request.Id} was not found.");
        }

        if (campaign.Status != "Scheduled")
        {
            return Error.Conflict("Campaign.InvalidStatus", $"Push campaign with status '{campaign.Status}' cannot be cancelled. Only 'Scheduled' campaigns can be cancelled.");
        }

        if (!string.IsNullOrEmpty(campaign.HangfireJobId))
        {
            notificationScheduler.CancelCampaign(campaign.HangfireJobId);
        }

        campaign.Cancel();

        await unitOfWork.CommitChangesAsync(cancellationToken);

        return Result.Success;
    }
}
