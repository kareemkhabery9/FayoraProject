using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fayora.Application.Common.Interfaces.Services.ChatbotModule;

public class ChatbotResponse
{
    public string? Text { get; set; }
    public List<ToolCallRequest>? ToolCalls { get; set; }
}

public class ToolCallRequest
{
    public string Name { get; set; } = null!;
    public string ArgumentsJson { get; set; } = null!;
    public string? Id { get; set; }
    public string? ThoughtSignature { get; set; }
}

public class ToolResponse
{
    public string Name { get; set; } = null!;
    public string ArgumentsJson { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? Id { get; set; }
    public string? ThoughtSignature { get; set; }
}

public interface IChatbotService
{
    Task<ChatbotResponse> GenerateResponseAsync(
        string userPrompt, 
        List<(string Role, string Content)> history, 
        List<ToolResponse>? toolResponses = null,
        CancellationToken cancellationToken = default);
}
