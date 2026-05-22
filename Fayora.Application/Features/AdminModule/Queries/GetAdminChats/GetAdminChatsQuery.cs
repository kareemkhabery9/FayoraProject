using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Contracts.AdminModule.LiveChatMonitoring;
using Fayora.Domain.Common.Results;
using System.Collections.Generic;

namespace Fayora.Application.Features.AdminModule.Queries.GetAdminChats;

public record GetAdminChatsQuery(
    int PageNumber,
    int PageSize) : IQuery<Result<List<GetAdminChatsResponse>>>;
