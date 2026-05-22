using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fayora.Domain.Entities.NotificationModule;
using Fayora.Contracts.AdminModule.Notifications;

namespace Fayora.Application.Common.Interfaces.Persistences.NotificationModule;

public interface INotificationRepository
{
    Task AddDeviceTokenAsync(DeviceToken token, CancellationToken ct);
    Task<DeviceToken?> GetDeviceTokenByTokenAsync(string token, CancellationToken ct);
    Task<List<string>> GetTokensByAudienceAsync(string targetAudience, CancellationToken ct);
    Task AddPushCampaignAsync(PushCampaign campaign, CancellationToken ct);
    Task<PushCampaign?> GetPushCampaignByIdAsync(Guid id, CancellationToken ct);
    Task<List<PushCampaign>> GetPushCampaignsPaginatedAsync(int pageNumber, int pageSize, CancellationToken ct);
    Task<PushCampaignStatsResponse> GetPushCampaignStatsAsync(CancellationToken ct);
}
