using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Contracts.AdminModule.ChatbotMonitoring;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.AdminModule.Queries.GetChatbotStats;

public record GetChatbotStatsQuery() : IQuery<Result<GetChatbotStatsResponse>>;
