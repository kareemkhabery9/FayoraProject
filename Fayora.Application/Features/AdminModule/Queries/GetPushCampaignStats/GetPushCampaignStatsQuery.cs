using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Contracts.AdminModule.Notifications;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AdminModule.Queries.GetPushCampaignStats;

public record GetPushCampaignStatsQuery() : IQuery<Result<PushCampaignStatsResponse>>;
