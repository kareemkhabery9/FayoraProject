using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Contracts.AdminModule.LiveChatMonitoring;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AdminModule.Queries.GetChatStats;

public record GetChatStatsQuery() : IQuery<Result<GetChatStatsResponse>>;
