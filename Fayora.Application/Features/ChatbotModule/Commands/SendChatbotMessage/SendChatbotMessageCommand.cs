using System;
using Fayora.Application.Common.Abstractions.Messaging;
using Fayora.Domain.Common.Results;

namespace Fayora.Application.Features.ChatbotModule.Commands.SendChatbotMessage;

public record SendChatbotMessageCommand(
    string DeviceId,
    string Content,
    Guid? SessionId = null,
    Guid? UserId = null
) : ICommand<Result<ChatbotMessageResult>>;
