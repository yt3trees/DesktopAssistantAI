using System.Collections.ObjectModel;

namespace DesktopAssistantAI.Models;

public record OpenAIApiConfig
{
    public required string ConfigurationName { get; set; }

    public required string Provider { get; set; }

    public required string ApiKey { get; set; }

    public string? AzureResourceUrl { get; set; }

    public string? AzureApiVersion { get; set; }
}

[Serializable]
public class OpenAIApiConfigCollection : ObservableCollection<OpenAIApiConfig>
{
    protected override void InsertItem(int index, OpenAIApiConfig item)
    {
        var baseName = item.ConfigurationName;
        int suffix = 1;

        while (Items.Any(x => x.ConfigurationName == item.ConfigurationName))
        {
            item.ConfigurationName = $"{baseName}-{suffix++}";
        }

        base.InsertItem(index, item);
    }
}

public static class OpenAIApiConfigManager
{
    public static OpenAIApiConfig GetOpenAIApiConfig(string configurationName)
    {
        var openAIApiConfig = App.Current.OpenAIApiConfigs.FirstOrDefault(item => item.ConfigurationName == configurationName);
        return openAIApiConfig;
    }

    public static OpenAIApiConfig GetOpenAIApiConfigFromAssistantId(string assistantId)
    {
        var selectAssistant = App.Current.AssistantsApiConfigs.FirstOrDefault(item => item.AssistantId == assistantId);
        var openAIApiConfig = App.Current.OpenAIApiConfigs.FirstOrDefault(item => item.ConfigurationName == selectAssistant?.ConfigurationName);
        return openAIApiConfig;
    }
}
