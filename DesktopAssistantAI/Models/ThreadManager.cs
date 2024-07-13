using DesktopAssistantAI.Helpers;
using OpenAI.ObjectModels.RequestModels;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace DesktopAssistantAI.Models;

public record ThreadManager
{
    public required string ThreadId { get; init; }

    public required string ResponseMessagePart { get; set; }

    public required int CreatedAt { get; init; }

    public required string ConfigurationName { get; set; }

    public required string AssistantId { get; set; }

    public string? AssistantName { get; set; }

    public ToolResources? ToolResources { get; set; }

    public bool Favorite { get; set; } = false;

    public string CreatedAtFormatted => StringOperationHelper.FormatDateTime(CreatedAt);
}

[Serializable]
public class ThreadManagerCollection : ObservableCollection<ThreadManager>
{
}

public class ThreadManagerService
{
    private readonly string _appFolderPath;

    public ThreadManagerService()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _appFolderPath = Path.Combine(documentsPath, Assembly.GetExecutingAssembly().GetName().Name, "threads");
        _ = Directory.CreateDirectory(_appFolderPath);
    }

    public void SaveThreadManager(ThreadManager threadManager)
    {
        string timestamp = StringOperationHelper.FormatDateTime(threadManager.CreatedAt, "yyyyMMddHHmmss");
        string fileName = $"thread_{timestamp}_{threadManager.ThreadId}.json";
        string filePath = Path.Combine(_appFolderPath, fileName);
        string json = JsonSerializer.Serialize(threadManager, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        File.WriteAllText(filePath, json);
    }

    public ThreadManagerCollection LoadThreadManagerCollection()
    {
        var collection = new ThreadManagerCollection();
        var files = Directory.GetFiles(_appFolderPath, "thread_*.json");
        foreach (var file in files)
        {
            string json = File.ReadAllText(file);
            var threadManager = JsonSerializer.Deserialize<ThreadManager>(json);
            if (threadManager != null)
            {
                collection.Add(threadManager);
            }
        }
        return collection;
    }

    public void DeleteThreadManager(ThreadManager threadManager)
    {
        string timestamp = StringOperationHelper.FormatDateTime(threadManager.CreatedAt, "yyyyMMddHHmmss");
        string fileName = $"thread_{timestamp}_{threadManager.ThreadId}.json";
        string filePath = Path.Combine(_appFolderPath, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
