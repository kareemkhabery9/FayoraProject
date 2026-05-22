using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.NotificationModule;
using Fayora.Contracts.AdminModule.Notifications;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AdminModule.Queries.GetPushCampaigns;

public class GetPushCampaignsQueryHandler(INotificationRepository notificationRepository)
    : IQueryHandler<GetPushCampaignsQuery, Result<List<PushCampaignResponse>>>
{
    public async Task<Result<List<PushCampaignResponse>>> Handle(GetPushCampaignsQuery request, CancellationToken cancellationToken)
    {
        var campaigns = await notificationRepository.GetPushCampaignsPaginatedAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var responseList = campaigns.Select(c => new PushCampaignResponse(
            c.Id,
            c.Title,
            c.Body,
            c.ImageUrl,
            c.TargetAudience,
            c.ScheduledAt,
            c.SentAt,
            c.Status,
            c.SuccessCount,
            c.FailureCount,
            c.CreatedAt
        )).ToList();

        return responseList;
    }
}
