using System.Collections.Generic;
using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Contracts.AdminModule.Notifications;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AdminModule.Queries.GetPushCampaigns;

public record GetPushCampaignsQuery(int PageNumber, int PageSize) : IQuery<Result<List<PushCampaignResponse>>>;
