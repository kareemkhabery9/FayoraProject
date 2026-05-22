using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Services.ChatbotModule;
using Fayora.Contracts.ChatbotModule;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.ChatbotModule.Queries.GetChatbotHistory;

public class GetChatbotHistoryQueryHandler(
    IChatbotInteractionService chatbotInteractionService)
    : IQueryHandler<GetChatbotHistoryQuery, Result<List<ChatbotMessageResponse>>>
{
    public async Task<Result<List<ChatbotMessageResponse>>> Handle(
        GetChatbotHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var result = await chatbotInteractionService.GetHistoryAsync(
            request.DeviceId,
            request.SessionId,
            cancellationToken);

        return result;
    }
}
