using DesktopAssistantAI.Models;
using OpenAI;
using OpenAI.Managers;

namespace DesktopAssistantAI.Services;

public class AssistantsApiService
{
    public static OpenAIService CreateOpenAIService(string configurationName)
    {
        var apiConfig = OpenAIApiConfigManager.GetOpenAIApiConfig(configurationName);

        OpenAIService openAiService = apiConfig.Provider == "OpenAI"
            ? new OpenAIService(new OpenAiOptions()
            {
                UseBeta = true,
                ProviderType = ProviderType.OpenAi,
                ApiKey = apiConfig.ApiKey,
            })
            : new OpenAIService(new OpenAiOptions()
            {
                UseBeta = true,
                ProviderType = ProviderType.Azure,
                ApiKey = apiConfig.ApiKey,
                BaseDomain = apiConfig.AzureResourceUrl,
                ApiVersion = apiConfig.AzureApiVersion,
                ValidateApiOptions = false,
            });

        return openAiService;
    }

    public static OpenAIService CreateOpenAIServiceFromAssistantId(string assistantId)
    {
        var apiConfig = OpenAIApiConfigManager.GetOpenAIApiConfigFromAssistantId(assistantId);

        OpenAIService openAiService = apiConfig.Provider == "OpenAI"
            ? new OpenAIService(new OpenAiOptions()
            {
                UseBeta = true,
                ProviderType = ProviderType.OpenAi,
                ApiKey = apiConfig.ApiKey,
            })
            : new OpenAIService(new OpenAiOptions()
            {
                UseBeta = true,
                ProviderType = ProviderType.Azure,
                ApiKey = apiConfig.ApiKey,
                BaseDomain = apiConfig.AzureResourceUrl,
                ApiVersion = apiConfig.AzureApiVersion,
                ValidateApiOptions = false,
            });

        return openAiService;
    }
}
