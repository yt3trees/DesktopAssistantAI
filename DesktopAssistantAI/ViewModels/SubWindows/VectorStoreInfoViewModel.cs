using DesktopAssistantAI.Helpers;
using DesktopAssistantAI.Models;
using DesktopAssistantAI.Services;
using Microsoft.Win32;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels.VectorStoreResponseModels;
using System.IO;

namespace DesktopAssistantAI.ViewModels.SubWindows;

public partial class VectorStoreInfoViewModel : ObservableObject
{
    [ObservableProperty]
    private FileObjects _vectorStoreFiles = new FileObjects();

    [ObservableProperty]
    private VectorStoreObjectResponse _vectorStoreObject = new VectorStoreObjectResponse();

    [ObservableProperty]
    private string _vectorStoreCreatedFormatted = string.Empty;

    [ObservableProperty]
    private string? _vectorStoreLastActiveFormatted = string.Empty;

    [ObservableProperty]
    private string _vectorStoreSize = string.Empty;

    [ObservableProperty]
    private string _uploadedFormatted = string.Empty;

    [ObservableProperty]
    private string _expiresAt = string.Empty;

    [ObservableProperty]
    private string _expiresPolicy = string.Empty;

    private string _configurationName = string.Empty;

    public VectorStoreInfoViewModel(VectorStoreObjectResponse vectorStoreObject, FileObjects fileObjects, string configurationName)
    {
        VectorStoreObject = vectorStoreObject;
        VectorStoreFiles = fileObjects;
        _configurationName = configurationName;

        var size = Math.Round(vectorStoreObject.UsageBytes / 1024f, 2);
        VectorStoreSize = $"{size.ToString("N2")} KB";

        VectorStoreCreatedFormatted = StringOperationHelper.FormatDateTime(vectorStoreObject.CreatedAt);

        VectorStoreLastActiveFormatted = StringOperationHelper.FormatDateTime((int)vectorStoreObject.LastActiveAt);

        if (vectorStoreObject.ExpiresAt != null)
        {
            ExpiresAt = StringOperationHelper.FormatDateTime((int)vectorStoreObject.ExpiresAt);
        }
        else
        {
            ExpiresAt = "Never";
        }

        if (vectorStoreObject.ExpiresAfter != null)
        {
            ExpiresPolicy = vectorStoreObject.ExpiresAfter.Days.ToString();
        }
        else
        {
            ExpiresPolicy = "Never";
        }
    }

    public VectorStoreInfoViewModel()
    {
    }

    [RelayCommand]
    private async Task OnAddButtonClick()
    {
        OpenFileDialog openFileDialog = new()
        {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Filter = "All files (*.*)|*.*"
        };
        if (openFileDialog.ShowDialog() != true)
        {
            return;
        }
        if (!File.Exists(openFileDialog.FileName))
        {
            return;
        }

        try
        {
            OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(_configurationName);

            string filePath = openFileDialog.FileName;
            string fileName = Path.GetFileName(filePath);
            byte[] fileBytes = File.ReadAllBytes(filePath);
            var fileUploadResponse = await openAiService.UploadFile("assistants", fileBytes, fileName);

            if (fileUploadResponse.Successful == true)
            {
                var createFileRequest = new CreateVectorStoreFileRequest { FileId = fileUploadResponse.Id };
                var fileObject = await openAiService.CreateVectorStoreFile(VectorStoreObject.Id, createFileRequest);
                if (fileObject.Successful == true)
                {
                    await MessageBoxHelper.ShowMessageAsync("Success", "File added successfully.");

                    VectorStoreFiles.Add(new FileObject
                    {
                        FileId = fileObject.Id,
                        FileName = fileName,
                        Uploaded = fileObject.CreatedAt,
                    });
                }
                else
                {
                    await MessageBoxHelper.ShowMessageAsync("Error", fileObject.Error.Message);
                }
            }
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
    }

    [RelayCommand]
    private async Task OnDeleteFileButtonClick(object parameter)
    {
        if (parameter is FileObject fileObject)
        {
            Wpf.Ui.Controls.MessageBoxResult confirmResult = await new Wpf.Ui.Controls.MessageBox
            {
                Title = "Confirm",
                Content = $"Do you want to delete {fileObject.FileName}?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            }.ShowDialogAsync();

            if (confirmResult != Wpf.Ui.Controls.MessageBoxResult.Primary)
            {
                return;
            }

            OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(_configurationName);
            var fileDeleteResponse = await openAiService.DeleteVectorStoreFile(VectorStoreObject.Id, fileObject.FileId);

            if ((fileDeleteResponse.Successful == true) && (fileDeleteResponse.IsDeleted == true))
            {
                await MessageBoxHelper.ShowMessageAsync("Success", "File deleted successfully.");
                VectorStoreFiles.Remove(fileObject);
            }
            else
            {
                await MessageBoxHelper.ShowMessageAsync("Error", fileDeleteResponse.Error.Message);
            }
        }
    }
    [RelayCommand]
    private async Task OnDeleteVectorStoreButtonClick(object parameter)
    {
        try
        {
            OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(_configurationName);

            var response = await openAiService.DeleteVectorStore(VectorStoreObject.Id);

            if (response.Successful == true)
            {
                await MessageBoxHelper.ShowMessageAsync("Success", "Delete vector store successfully.");
            }
            else
            {
                await MessageBoxHelper.ShowMessageAsync("Error", response.Error.Message);
            }
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
    }
    [RelayCommand]
    private async Task OnRenameVectorStoreButtonClick(object parameter)
    {
        OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(_configurationName);

        UpdateVectorStoreRequest request = new UpdateVectorStoreRequest
        {
            Name = VectorStoreObject.Name,
        };

        var response = await openAiService.ModifyVectorStore(VectorStoreObject.Id, request);

        if (response.Successful == true)
        {
            await MessageBoxHelper.ShowMessageAsync("Success", "Rename vector store name successfully.");
        }
        else
        {
            await MessageBoxHelper.ShowMessageAsync("Error", response.Error.Message);
        }
    }
}
