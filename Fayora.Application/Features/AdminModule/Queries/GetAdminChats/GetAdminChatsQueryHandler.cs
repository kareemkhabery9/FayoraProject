using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Application.Common.Interfaces.Persistences.AdminModule;
using Fayora.Contracts.AdminModule.LiveChatMonitoring;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Features.AdminModule.Queries.GetAdminChats;

public class GetAdminChatsQueryHandler(IAdminRepository adminRepository) : IQueryHandler<GetAdminChatsQuery, Result<List<GetAdminChatsResponse>>>
{
    public async Task<Result<List<GetAdminChatsResponse>>> Handle(GetAdminChatsQuery request, CancellationToken cancellationToken)
    {
        var chats = await adminRepository.GetAdminChatsAsync(request.PageNumber, request.PageSize, cancellationToken);
        return chats;
    }
}
