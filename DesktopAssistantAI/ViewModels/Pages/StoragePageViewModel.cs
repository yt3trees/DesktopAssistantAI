using DesktopAssistantAI.Helpers;
using DesktopAssistantAI.Models;
using DesktopAssistantAI.Services;
using DesktopAssistantAI.ViewModels.Dialogs;
using DesktopAssistantAI.Views.Dialogs;
using DesktopAssistantAI.Views.SubWindows;
using Microsoft.Win32;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using System.Collections.ObjectModel;
using System.IO;

namespace DesktopAssistantAI.ViewModels.Pages;

public partial class StoragePageViewModel : ObservableObject
{
    private const int PageSize = 6;

    [ObservableProperty]
    private OpenAIApiConfigCollection _openAIApiConfigItems;

    [ObservableProperty]
    private OpenAIApiConfig _selectedOpenAIApiConfigItem;

    [ObservableProperty]
    private bool _isFilesToggleButtonChecked = true;
    partial void OnIsFilesToggleButtonCheckedChanged(bool oldValue, bool newValue)
    {
        IsVectorStoresToggleButtonChecked = !newValue;
        CurrentPage = 1;
        UpdateCurrentPageItems();
    }

    [ObservableProperty]
    private bool _isVectorStoresToggleButtonChecked = false;
    partial void OnIsVectorStoresToggleButtonCheckedChanged(bool oldValue, bool newValue)
    {
        IsFilesToggleButtonChecked = !newValue;
        CurrentPage = 1;
        UpdateCurrentPageItems();
    }

    [ObservableProperty]
    private ObservableCollection<Vector> _vectorStoreList = new ObservableCollection<Vector>();

    [ObservableProperty]
    private ObservableCollection<File> _fileList = new ObservableCollection<File>();

    [ObservableProperty]
    private ObservableCollection<Vector> _currentPageVectorStores = new ObservableCollection<Vector>();

    [ObservableProperty]
    private ObservableCollection<File> _currentPageFiles = new ObservableCollection<File>();

    [ObservableProperty]
    private bool _isProgressBarActive = false;

    [ObservableProperty]
    private int _currentPage = 1;

    public int TotalPages => (int)Math.Ceiling((double)(IsFilesToggleButtonChecked ? FileList.Count : VectorStoreList.Count) / PageSize);

    public StoragePageViewModel()
    {
        _openAIApiConfigItems = App.Current.OpenAIApiConfigs;
    }

    [RelayCommand]
    private async Task OnUpdateButtonClick()
    {
        try
        {
            IsProgressBarActive = true;
            OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

            if (IsVectorStoresToggleButtonChecked)
            {
                PaginationRequest request = new PaginationRequest();
                var response = await openAiService.ListVectorStores(request);
                if (response.Successful == true)
                {
                    VectorStoreList.Clear();
                    foreach (var item in response.Data)
                    {
                        VectorStoreList.Add(new Vector
                        {
                            Name = item.Name,
                            Id = item.Id,
                            Size = $"{Math.Round(item.UsageBytes / 1024f, 2).ToString("N2")} KB",
                            CreatedAt = StringOperationHelper.FormatDateTime((int)item.CreatedAt),
                        });
                    }
                }
            }
            else if (IsFilesToggleButtonChecked)
            {
                var response = await openAiService.ListFile();
                if (response.Successful == true)
                {
                    FileList.Clear();
                    foreach (var item in response.Data)
                    {
                        FileList.Add(new File
                        {
                            Name = item.FileName,
                            Id = item.Id,
                            Size = $"{Math.Round((decimal)(item.Bytes / 1024f), 2).ToString("N2")} KB",
                            CreatedAt = StringOperationHelper.FormatDateTime((int)item.CreatedAt),
                            Purpose = item.Purpose,
                        });
                    }
                }
            }
            UpdateCurrentPageItems();
        }
        catch (Exception ex)
        {
            await new Wpf.Ui.Controls.MessageBox
            {
                Title = "Error",
                Content = ex.Message,
            }.ShowDialogAsync();
        }
        finally
        {
            IsProgressBarActive = false;
        }
    }

    [RelayCommand]
    private async Task OnOpenVectorStore(object parameter)
    {
        try
        {
            IsProgressBarActive = true;
            if (parameter is Vector vector)
            {
                OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

                VectorStoreFileListRequest vectorRequest = new VectorStoreFileListRequest();

                var files = await openAiService.ListVectorStoreFiles(vector.Id, vectorRequest);
                var vectorFileList = new FileObjects();

                foreach (var file in files.Data)
                {
                    var fileInfo = await openAiService.RetrieveFile(file.Id);
                    vectorFileList.Add(new FileObject
                    {
                        FileId = file.Id,
                        FileName = fileInfo.FileName,
                        Uploaded = fileInfo.CreatedAt,
                    });
                }

                var vectorStoreInfo = await openAiService.RetrieveVectorStore(vector.Id);

                IsProgressBarActive = false;
                var window = new VectorStoreInfo(vectorStoreInfo, vectorFileList, SelectedOpenAIApiConfigItem.ConfigurationName);
                window.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            Wpf.Ui.Controls.MessageBoxResult result = await new Wpf.Ui.Controls.MessageBox
            {
                Title = "Error",
                Content = ex.Message,
                CloseButtonText = "Close"
            }.ShowDialogAsync();
        }
        finally
        {
            IsProgressBarActive = false;
        }
    }

    [RelayCommand]
    private async Task OnCreateVectorStoreButtonClick(object owner)
    {
        try
        {
            var viewModel = new TextInputDialogViewModel("Vector Store Name:");
            var window = new TextInputDialog(viewModel);
            window.Owner = (Window)owner;
            window.ShowDialog();

            IsProgressBarActive = true;

            if (window.DialogResult == true)
            {
                string vectorStoreName = window.ResponseText;

                OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

                var createVectorStoreRequest = new CreateVectorStoreRequest { Name = vectorStoreName };
                var requestResponse = await openAiService.CreateVectorStore(createVectorStoreRequest);

                if (requestResponse.Successful == true)
                {
                    VectorStoreList.Add(new Vector
                    {
                        Name = vectorStoreName,
                        Id = requestResponse.Id,
                        Size = $"{Math.Round(requestResponse.UsageBytes / 1024f, 2).ToString("N2")} KB",
                        CreatedAt = StringOperationHelper.FormatDateTime((int)requestResponse.CreatedAt),
                    });

                    await new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "Successful",
                        Content = "Add Vector Store Successful.",
                    }.ShowDialogAsync();
                }
                else
                {
                    throw new Exception(requestResponse.Error.Message);
                }
            }
            UpdateCurrentPageItems();
        }
        catch (Exception ex)
        {
            Wpf.Ui.Controls.MessageBoxResult result = await new Wpf.Ui.Controls.MessageBox
            {
                Title = "Error",
                Content = ex.Message,
                CloseButtonText = "Close"
            }.ShowDialogAsync();
        }
        finally
        {
            IsProgressBarActive = false;
        }
    }

    [RelayCommand]
    private async Task OnUploadFileButtonClick()
    {
        OpenFileDialog openFileDialog =
            new()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "All files (*.*)|*.*"
            };
        if (openFileDialog.ShowDialog() != true)
        {
            return;
        }
        if (!System.IO.File.Exists(openFileDialog.FileName))
        {
            return;
        }

        try
        {
            OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

            string filePath = openFileDialog.FileName;
            string fileName = Path.GetFileName(filePath);
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileUploadResponse = await openAiService.UploadFile("assistants", fileBytes, fileName);

            if (fileUploadResponse.Successful == true)
            {
                Wpf.Ui.Controls.MessageBoxResult result = await new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Success",
                    Content = "File upload successfully.",
                    CloseButtonText = "Close",
                }.ShowDialogAsync();

                FileList.Add(new File
                {
                    Name = fileName,
                    Id = fileUploadResponse.Id,
                    Size = $"{Math.Round(fileUploadResponse.Bytes / 1024f, 2).ToString("N2")} KB",
                    CreatedAt = StringOperationHelper.FormatDateTime((int)fileUploadResponse.CreatedAt),
                    Purpose = fileUploadResponse.Purpose
                });
            }
            else
            {
                Wpf.Ui.Controls.MessageBoxResult result = await new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Error",
                    Content = fileUploadResponse.Error.Message,
                    CloseButtonText = "Close",
                }.ShowDialogAsync();
            }
            UpdateCurrentPageItems();
        }
        catch (Exception ex)
        {
            Wpf.Ui.Controls.MessageBoxResult result = await new Wpf.Ui.Controls.MessageBox
            {
                Title = "Error",
                Content = ex.Message,
                CloseButtonText = "Close"
            }.ShowDialogAsync();
        }
    }

    [RelayCommand]
    private async Task OnDeleteFile(object parameter)
    {
        try
        {
            if (parameter is File file)
            {
                Wpf.Ui.Controls.MessageBoxResult messageResult = await new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Confirm",
                    Content = $"Do you want to delete \"{file.Name}\"?",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel"
                }.ShowDialogAsync();

                if (messageResult != Wpf.Ui.Controls.MessageBoxResult.Primary)
                {
                    return;
                }

                IsProgressBarActive = true;

                OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

                var deleteFileResponse = await openAiService.DeleteFile(file.Id);

                if (deleteFileResponse.Successful == true)
                {
                    FileList.Remove(file);

                    Wpf.Ui.Controls.MessageBoxResult result = await new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "Success",
                        Content = "File delete successfully.",
                        CloseButtonText = "Close",
                    }.ShowDialogAsync();
                }
                else
                {
                    Wpf.Ui.Controls.MessageBoxResult result = await new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "Error",
                        Content = deleteFileResponse.Error.Message,
                        CloseButtonText = "Close",
                    }.ShowDialogAsync();
                }
            }
            UpdateCurrentPageItems();
        }
        catch (Exception ex)
        {
            Wpf.Ui.Controls.MessageBoxResult result = await new Wpf.Ui.Controls.MessageBox
            {
                Title = "Error",
                Content = ex.Message,
                CloseButtonText = "Close"
            }.ShowDialogAsync();
        }
        finally
        {
            IsProgressBarActive = false;
        }
    }

    [RelayCommand]
    private async Task OnOpenFileInfo(object parameter)
    {
        try
        {
            string fileId = string.Empty;
            if (parameter is File file)
            {
                fileId = file.Id;
            }

            OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

            var fileResponse = await openAiService.RetrieveFile(fileId);

            await MessageBoxHelper.ShowMessageAsync("File", "FileName: " + fileResponse.FileName +
                "\r\nStatus: " + fileResponse.Status +
                "\r\nPurpose: " + fileResponse.Purpose +
                "\r\nSize: " + $"{Math.Round((decimal)(fileResponse.Bytes / 1024f), 2).ToString("N2")} KB");
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
    }

    [RelayCommand]
    private async Task OnDownloadFile(object parameter)
    {
        try
        {
            IsProgressBarActive = true;
            if (parameter is File res)
            {
                string annotation = res.Name;
                string fileName = Path.GetFileName(annotation);

                OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

                string fileId = res.Id;
                OpenAI.ObjectModels.ResponseModels.FileResponseModels.FileContentResponse<byte[]> response = await openAiService.RetrieveFileContent<byte[]>(fileId);

                if (response.Successful)
                {
                    byte[] file = response.Content;

                    SaveFileDialog openFileDialog = new()
                    {
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        Filter = "All files (*.*)|*.*"
                    };

                    var invalidChars =
                        new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

                    openFileDialog.FileName = string.Join(
                            "_",
                            fileName.Split(invalidChars.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                        ).Trim();

                    if (openFileDialog.ShowDialog() != true)
                    {
                        return;
                    }

                    System.IO.File.WriteAllBytes(openFileDialog.FileName, file);

                    await MessageBoxHelper.ShowMessageAsync("Success", $"Save to {openFileDialog.FileName}");
                }
            }
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
        finally
        {
            IsProgressBarActive = false;
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            UpdateCurrentPageItems();
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            UpdateCurrentPageItems();
        }
    }

    private void UpdateCurrentPageItems()
    {
        if (IsFilesToggleButtonChecked)
        {
            var filesToShow = FileList.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
            CurrentPageFiles.Clear();

            foreach (var file in filesToShow)
            {
                CurrentPageFiles.Add(file);
            }
        }
        else if (IsVectorStoresToggleButtonChecked)
        {
            var vectorsToShow = VectorStoreList.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
            CurrentPageVectorStores.Clear();

            foreach (var vector in vectorsToShow)
            {
                CurrentPageVectorStores.Add(vector);
            }
        }

        OnPropertyChanged(nameof(TotalPages));
    }

    public class Vector
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public string Size { get; set; }

        public string CreatedAt { get; set; }
    }

    public class File
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public string Size { get; set; }

        public string CreatedAt { get; set; }

        public string Purpose { get; set; }
    }
}
