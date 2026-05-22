using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Contracts.AdminModule.ChatbotMonitoring;
using Fayora.Domain.Common.Results;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetChatbotStats;

public class GetChatbotStatsQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetChatbotStatsQuery, Result<GetChatbotStatsResponse>>
{
    public async Task<Result<GetChatbotStatsResponse>> Handle(GetChatbotStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await adminRepository.GetChatbotStatsAsync(cancellationToken);
        return stats;
    }
}
