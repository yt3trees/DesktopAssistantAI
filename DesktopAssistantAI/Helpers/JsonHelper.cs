using DesktopAssistantAI.Models;
using System.Text.Json;

namespace DesktopAssistantAI.Helpers;

public static class JsonHelper
{
    public static string SerializeAssistantsApiConfigCollection(AssistantsApiConfigCollection collection)
    {
        return JsonSerializer.Serialize(collection);
    }

    public static AssistantsApiConfigCollection DeserializeAssistantsApiConfigCollection(string json)
    {
        return JsonSerializer.Deserialize<AssistantsApiConfigCollection>(json) ?? new AssistantsApiConfigCollection();
    }

    public static string SerializeOpenAIApiConfigCollection(OpenAIApiConfigCollection collection)
    {
        return JsonSerializer.Serialize(collection);
    }

    public static OpenAIApiConfigCollection DeserializeOpenAIApiConfigCollection(string json)
    {
        return JsonSerializer.Deserialize<OpenAIApiConfigCollection>(json) ?? new OpenAIApiConfigCollection();
    }

    public static string SerializeAvatarConfigCollection(AvatarConfigCollection collection)
    {
        return JsonSerializer.Serialize(collection);
    }

    public static AvatarConfigCollection DeserializeAvatarConfigCollection(string json)
    {
        return JsonSerializer.Deserialize<AvatarConfigCollection>(json) ?? new AvatarConfigCollection();
    }
}
