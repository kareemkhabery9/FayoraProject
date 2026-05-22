namespace Fayora.Application.Common.Interfaces.Services.ChatbotModule;

public interface IChatbotServiceFactory
{
    IChatbotService GetService(string provider);
}
