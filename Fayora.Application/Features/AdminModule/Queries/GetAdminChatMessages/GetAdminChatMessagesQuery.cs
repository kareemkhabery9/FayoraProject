using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Contracts.AdminModule.LiveChatMonitoring;
using Fayora.Domain.Common.Results;
using System;
using System.Collections.Generic;

namespace Fayora.Application.Features.AdminModule.Queries.GetAdminChatMessages;

public record GetAdminChatMessagesQuery(Guid ChatId) : IQuery<Result<List<GetAdminChatMessagesResponse>>>;
