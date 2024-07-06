using System.Collections.ObjectModel;

namespace DesktopAssistantAI.Models;

public record AssistantConfigMerger
{
    public required string ConfigurationName { get; set; }

    public required string AssistantId { get; init; }

    public required string Provider { get; set; }

    public required string ApiKey { get; set; }

    public string? AzureResourceUrl { get; set; }

    public string? AzureApiVersion { get; set; }
}

public static class ConfigCombiner
{
    public static List<AssistantConfigMerger> CombineConfigs(
        ObservableCollection<OpenAIApiConfig> openAIApiConfigs,
        ObservableCollection<AssistantsApiConfig> assistantsApiConfigs)
    {
        var combinedConfigs = new List<AssistantConfigMerger>();

        foreach (var assistant in assistantsApiConfigs)
        {
            var relatedOpenAiConfig = openAIApiConfigs
                .FirstOrDefault(o => o.ConfigurationName == assistant.ConfigurationName);

            if (relatedOpenAiConfig != null)
            {
                combinedConfigs.Add(new AssistantConfigMerger
                {
                    ConfigurationName = assistant.ConfigurationName,
                    AssistantId = assistant.AssistantId,
                    Provider = relatedOpenAiConfig.Provider,
                    ApiKey = relatedOpenAiConfig.ApiKey,
                    AzureResourceUrl = relatedOpenAiConfig.AzureResourceUrl,
                    AzureApiVersion = relatedOpenAiConfig?.AzureApiVersion,
                });
            }
        }

        return combinedConfigs;
    }

    public static AssistantConfigMerger? MergeConfigBySelectedAssistantId()
    {
        var savedAssistantId = Properties.Settings.Default.SelectedAssistantId;
        var assistantsApiConfigs = App.Current.AssistantsApiConfigs;
        var openAIApiConfigs = App.Current.OpenAIApiConfigs;

        var assistant = assistantsApiConfigs.FirstOrDefault(a => a.AssistantId == savedAssistantId);

        if (assistant != null)
        {
            var relatedOpenAiConfig = openAIApiConfigs
                .FirstOrDefault(o => o.ConfigurationName == assistant.ConfigurationName);

            if (relatedOpenAiConfig != null)
            {
                return new AssistantConfigMerger
                {
                    ConfigurationName = assistant.ConfigurationName,
                    AssistantId = assistant.AssistantId,
                    Provider = relatedOpenAiConfig.Provider,
                    ApiKey = relatedOpenAiConfig.ApiKey,
                    AzureResourceUrl = relatedOpenAiConfig.AzureResourceUrl,
                    AzureApiVersion = relatedOpenAiConfig?.AzureApiVersion,
                };
            }
        }

        return null;
    }

    public static AssistantConfigMerger? MergeConfigByAssistantId(string assistantId)
    {
        var savedAssistantId = assistantId;
        var assistantsApiConfigs = App.Current.AssistantsApiConfigs;
        var openAIApiConfigs = App.Current.OpenAIApiConfigs;

        var assistant = assistantsApiConfigs.FirstOrDefault(a => a.AssistantId == savedAssistantId);

        if (assistant != null)
        {
            var relatedOpenAiConfig = openAIApiConfigs
                .FirstOrDefault(o => o.ConfigurationName == assistant.ConfigurationName);

            if (relatedOpenAiConfig != null)
            {
                return new AssistantConfigMerger
                {
                    ConfigurationName = assistant.ConfigurationName,
                    AssistantId = assistant.AssistantId,
                    Provider = relatedOpenAiConfig.Provider,
                    ApiKey = relatedOpenAiConfig.ApiKey,
                    AzureResourceUrl = relatedOpenAiConfig.AzureResourceUrl,
                    AzureApiVersion = relatedOpenAiConfig?.AzureApiVersion,
                };
            }
        }

        return null;
    }
}
