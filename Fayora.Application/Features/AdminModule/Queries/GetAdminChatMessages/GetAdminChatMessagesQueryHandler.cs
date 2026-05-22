using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Contracts.AdminModule.LiveChatMonitoring;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetAdminChatMessages;

public class GetAdminChatMessagesQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetAdminChatMessagesQuery, Result<List<GetAdminChatMessagesResponse>>>
{
    public async Task<Result<List<GetAdminChatMessagesResponse>>> Handle(GetAdminChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await adminRepository.GetAdminChatMessagesAsync(request.ChatId, cancellationToken);
        return messages;
    }
}
