using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Contracts.AdminModule.ChatbotMonitoring;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetChatbotSessions;

public class GetChatbotSessionsQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetChatbotSessionsQuery, Result<List<GetChatbotSessionsResponse>>>
{
    public async Task<Result<List<GetChatbotSessionsResponse>>> Handle(GetChatbotSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await adminRepository.GetChatbotSessionsAsync(
            request.PageNumber, request.PageSize, request.UserIdFilter, request.FromDate, request.ToDate, cancellationToken);
        return sessions;
    }
}
