using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fayora.Application.Common.Interfaces.Persistences.NotificationModule;
using Fayora.Contracts.AdminModule.Notifications;
using Fayora.Domain.Entities.NotificationModule;
using Fayora.Domain.Enums.IdentityModule;
using Microsoft.EntityFrameworkCore;

namespace Fayora.Infrastructure.Persistence.Repositories.NotificationModule;

public class NotificationRepository(ApplicationDbContext context) : INotificationRepository
{
    public async Task AddDeviceTokenAsync(DeviceToken token, CancellationToken ct)
    {
        await context.DeviceTokens.AddAsync(token, ct);
    }

    public async Task<DeviceToken?> GetDeviceTokenByTokenAsync(string token, CancellationToken ct)
    {
        return await context.DeviceTokens.FirstOrDefaultAsync(d => d.Token == token, ct);
    }

    public async Task<List<string>> GetTokensByAudienceAsync(string targetAudience, CancellationToken ct)
    {
        if (string.Equals(targetAudience, "All", StringComparison.OrdinalIgnoreCase))
        {
            return await context.DeviceTokens
                .Select(dt => dt.Token)
                .ToListAsync(ct);
        }

        Role? targetRole = targetAudience.ToLower() switch
        {
            "tourist" => Role.Tourist,
            "tourguide" => Role.TourGuide,
            "tourcompany" => Role.TourCompany,
            "travelagency" => Role.TourCompany,
            "unitowner" => Role.UnitOwner,
            _ => null
        };

        if (targetRole == null)
        {
            return new List<string>();
        }

        return await context.DeviceTokens
            .Where(dt => dt.UserId != null && context.Users.Any(u => u.Id == dt.UserId && u.Roles.HasValue && (u.Roles.Value & targetRole.Value) != 0))
            .Select(dt => dt.Token)
            .ToListAsync(ct);
    }

    public async Task AddPushCampaignAsync(PushCampaign campaign, CancellationToken ct)
    {
        await context.PushCampaigns.AddAsync(campaign, ct);
    }

    public async Task<PushCampaign?> GetPushCampaignByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.PushCampaigns.FirstOrDefaultAsync(pc => pc.Id == id, ct);
    }

    public async Task<List<PushCampaign>> GetPushCampaignsPaginatedAsync(int pageNumber, int pageSize, CancellationToken ct)
    {
        return await context.PushCampaigns
            .OrderByDescending(pc => pc.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<PushCampaignStatsResponse> GetPushCampaignStatsAsync(CancellationToken ct)
    {
        var campaigns = await context.PushCampaigns.AsNoTracking().ToListAsync(ct);

        var totalCampaigns = campaigns.Count;
        var scheduledCampaigns = campaigns.Count(c => c.Status == "Scheduled");
        var sentCampaigns = campaigns.Count(c => c.Status == "Sent");
        var failedCampaigns = campaigns.Count(c => c.Status == "Failed");
        var totalSuccessDeliveries = campaigns.Where(c => c.Status == "Sent").Sum(c => c.SuccessCount);
        var totalFailureDeliveries = campaigns.Where(c => c.Status == "Sent").Sum(c => c.FailureCount);

        return new PushCampaignStatsResponse(
            totalCampaigns,
            scheduledCampaigns,
            sentCampaigns,
            failedCampaigns,
            totalSuccessDeliveries,
            totalFailureDeliveries
        );
    }
}
