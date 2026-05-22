using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Contracts.AdminModule.ChatbotMonitoring;
using Fayora.Domain.Common.Results;
using System;
using System.Collections.Generic;

namespace Fayora.Application.Features.AdminModule.Queries.GetChatbotSessionMessages;

public record GetChatbotSessionMessagesQuery(Guid SessionId) : IQuery<Result<List<GetChatbotSessionMessagesResponse>>>;
