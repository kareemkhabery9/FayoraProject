using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fayora.Application.Features.ChatbotModule.Commands.SendChatbotMessage;
using Fayora.Contracts.ChatbotModule;

namespace Fayora.Application.Common.Interfaces.Services.ChatbotModule;

public interface IChatbotInteractionService
{
    Task<ChatbotMessageResult> ProcessMessageAsync(
        string deviceId,
        string content,
        Guid? sessionId,
        Guid? userId,
        CancellationToken cancellationToken);

    Task<List<ChatbotMessageResponse>> GetHistoryAsync(
        string deviceId,
        Guid? sessionId,
        CancellationToken cancellationToken);
}
