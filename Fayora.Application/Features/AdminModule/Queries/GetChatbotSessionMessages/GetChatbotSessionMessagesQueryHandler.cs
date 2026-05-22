using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Contracts.AdminModule.ChatbotMonitoring;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetChatbotSessionMessages;

public class GetChatbotSessionMessagesQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetChatbotSessionMessagesQuery, Result<List<GetChatbotSessionMessagesResponse>>>
{
    public async Task<Result<List<GetChatbotSessionMessagesResponse>>> Handle(GetChatbotSessionMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await adminRepository.GetChatbotSessionMessagesAsync(request.SessionId, cancellationToken);
        return messages;
    }
}
