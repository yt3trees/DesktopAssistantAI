using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace DesktopAssistantAI.Models;

public record AvatarConfig
{
    public required string AvatarName { get; set; }

    private string _creationType = "Custom";

    public string CreationType
    {
        get => _creationType;
        init
        {
            if (!CreationTypeList.Contains(value))
            {
                throw new ArgumentException($"Value must be one of the following: {string.Join(", ", CreationTypeList)}");
            }
            _creationType = value;
        }
    }

    private string _displayType = "Custom";

    /// <summary>
    /// "Image" or "Animation"
    /// </summary>
    public required string DisplayType
    {
        get => _displayType;
        init
        {
            if (!DisplayTypeList.Contains(value))
            {
                throw new ArgumentException($"Value must be one of the following: {string.Join(", ", DisplayTypeList)}");
            }
            _displayType = value;
        }
    }

    public required string AvatarImagePath { get; set; }

    private static readonly List<string> CreationTypeList = new List<string> { "BuiltIn", "Custom" };

    private static readonly List<string> DisplayTypeList = new List<string> { "Image", "Animation" };
}

[Serializable]
public class AvatarConfigCollection : ObservableCollection<AvatarConfig>
{
    public AvatarConfigCollection()
    {
    }

    protected override void InsertItem(int index, AvatarConfig item)
    {
        var baseName = item.AvatarName;
        int suffix = 1;

        while (Items.Any(x => x.AvatarName == item.AvatarName))
        {
            item.AvatarName = $"{baseName}-{suffix++}";
        }

        base.InsertItem(index, item);
        SortCollection();
    }

    private void SortCollection()
    {
        var sorted = this.OrderBy(x => x.CreationType == "BuiltIn" ? 0 : 1)
                         .ThenBy(x => x.AvatarName)
                         .ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            if (!this[i].Equals(sorted[i]))
            {
                Move(IndexOf(sorted[i]), i);
            }
        }
    }
}

public class AvatarManagerService
{
    private readonly string _appFolderPath;

    public bool Successful { get; private set; } = false;

    public AvatarManagerService()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _appFolderPath = Path.Combine(documentsPath, Assembly.GetExecutingAssembly().GetName().Name, "avatar");
        _ = Directory.CreateDirectory(_appFolderPath);
    }

    public string? SetFile()
    {
        OpenFileDialog openFileDialog =
            new()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "PNG files (*.png)|*.png"
            };
        if (openFileDialog.ShowDialog() != true)
        {
            return null;
        }
        if (!System.IO.File.Exists(openFileDialog.FileName))
        {
            return null;
        }

        string fileName = Path.GetFileName(openFileDialog.FileName);
        string filePath = Path.Combine(_appFolderPath, fileName);

        int counter = 1;
        while (File.Exists(filePath))
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
            string extension = Path.GetExtension(openFileDialog.FileName);
            fileName = $"{fileNameWithoutExtension}({counter++}){extension}";
            filePath = Path.Combine(_appFolderPath, fileName);
        }

        try
        {
            System.IO.File.Copy(openFileDialog.FileName, filePath, overwrite: false);
            Successful = true;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while copying the file: " + ex.Message);
        }

        return filePath;
    }

    public void DeleteFile(AvatarConfig config)
    {
        try
        {
            File.Delete(config.AvatarImagePath);

            if (!File.Exists(config.AvatarImagePath))
            {
                Successful = true;
            }
            else
            {
                throw new Exception("Failed to delete file.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while delete the file: " + ex.Message);
        }
    }
}
