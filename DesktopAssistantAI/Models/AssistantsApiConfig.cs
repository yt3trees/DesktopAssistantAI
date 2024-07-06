using System.Collections.ObjectModel;

namespace DesktopAssistantAI.Models;

public record AssistantsApiConfig
{
    public required string AssistantName { get; set; }

    public required string ConfigurationName { get; set; }

    public required string AssistantId { get; set; }
}

[Serializable]
public class AssistantsApiConfigCollection : ObservableCollection<AssistantsApiConfig>
{
}

public static class AssistantsApiConfigManager
{
    /// <summary>
    /// Obtains various configurations for the selected Assistant.
    /// </summary>
    public static AssistantsApiConfig GetSelectedAssistantConfig()
    {
        var savedAssistantId = Properties.Settings.Default.SelectedAssistantId;
        var assistantsApiConfigs = App.Current.AssistantsApiConfigs;

        if (!string.IsNullOrEmpty(savedAssistantId))
        {
            var selectAssistant = assistantsApiConfigs.FirstOrDefault(item => item.AssistantId == savedAssistantId);
            return selectAssistant;
        }

        return null;
    }

    public static string? GetApiKeyForAssistantConfig(AssistantsApiConfig assistantConfig)
    {
        var openApiConfigs = App.Current.OpenAIApiConfigs;
        var openApiConfig = openApiConfigs.FirstOrDefault(config => config.ConfigurationName == assistantConfig.ConfigurationName);
        return openApiConfig?.ApiKey;
    }

    public static string? GetProviderForAssistantConfig(AssistantsApiConfig assistantConfig)
    {
        var openApiConfigs = App.Current.OpenAIApiConfigs;
        var openApiConfig = openApiConfigs.FirstOrDefault(config => config.ConfigurationName == assistantConfig.ConfigurationName);
        return openApiConfig?.Provider;
    }
}
