using System;
using Fayora.Application.Common.Interfaces.Services.ChatbotModule;
using Microsoft.Extensions.DependencyInjection;

namespace Fayora.Infrastructure.Services.Chatbot;

public class ChatbotServiceFactory : IChatbotServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ChatbotServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IChatbotService GetService(string provider)
    {
        return provider.ToLower() switch
        {
            "openai" => _serviceProvider.GetRequiredService<OpenAIChatbotService>(),
            "gemini" or _ => _serviceProvider.GetRequiredService<GeminiChatbotService>()
        };
    }
}
