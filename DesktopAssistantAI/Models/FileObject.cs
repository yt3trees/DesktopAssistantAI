using DesktopAssistantAI.Helpers;
using System.Collections.ObjectModel;

namespace DesktopAssistantAI.Models;

public record FileObject
{
    public required string FileId { get; init; }

    public required string FileName { get; init; }

    public int? Uploaded { get; init; }

    public string? Type { get; init; }

    public string UploadedFormatted => Uploaded.HasValue
        ? StringOperationHelper.FormatDateTime((int)Uploaded.Value)
        : "None";
}

public class FileObjects : ObservableCollection<FileObject>
{
}
