using System.Threading;
using System.Threading.Tasks;
using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.NotificationModule;
using Fayora.Contracts.AdminModule.Notifications;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AdminModule.Queries.GetPushCampaignStats;

public class GetPushCampaignStatsQueryHandler(INotificationRepository notificationRepository)
    : IQueryHandler<GetPushCampaignStatsQuery, Result<PushCampaignStatsResponse>>
{
    public async Task<Result<PushCampaignStatsResponse>> Handle(GetPushCampaignStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await notificationRepository.GetPushCampaignStatsAsync(cancellationToken);
        return stats;
    }
}
