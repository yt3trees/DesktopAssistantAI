using DesktopAssistantAI.Helpers;
using DesktopAssistantAI.Models;
using DesktopAssistantAI.Services;
using DesktopAssistantAI.ViewModels.Dialogs;
using DesktopAssistantAI.Views.Dialogs;
using DesktopAssistantAI.Views.SubWindows;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels.VectorStoreResponseModels;
using OpenAI.ObjectModels.SharedModels;
using System.Collections.ObjectModel;

namespace DesktopAssistantAI.ViewModels.Pages;

public partial class ConfigurationPageViewModel : ObservableObject
{
    [ObservableProperty]
    private AssistantsApiConfigCollection _assistantsApiConfigItems;
    partial void OnAssistantsApiConfigItemsChanged(AssistantsApiConfigCollection value)
    {
        FilterAssistantsApiConfigItems();
    }

    [ObservableProperty]
    private AssistantsApiConfig _selectedAssistantsApiItem;

    partial void OnSelectedAssistantsApiItemChanged(AssistantsApiConfig value)
    {
        if (value != null)
        {
            SelectedOpenAIApiConfigItem = OpenAIApiConfigManager.GetOpenAIApiConfig(SelectedOpenAIApiConfigItem.ConfigurationName);
            AssistantName = value.AssistantName;
            AssistantId = value.AssistantId;

            RetrieveAssistantButtonEnable = true;
        }
        else
        {
            AssistantId = "";
            AssistantName = "";
            RetrieveAssistantButtonEnable = false;
        }

        Instructions = "";
        Model = "";
        Temperature = 1;
        TopP = 1;
        ToolFileSearch = false;
        ToolCodeInterpreter = false;
        AssistantInfoEnable = false;
        VectorStoreVisible = false;
        CreateAssistantButtonEnable = false;
        CodeInterpreterFileIds.Clear();
    }

    [ObservableProperty]
    private OpenAIApiConfigCollection _openAIApiConfigItems;

    [ObservableProperty]
    private OpenAIApiConfig _selectedOpenAIApiConfigItem;

    partial void OnSelectedOpenAIApiConfigItemChanged(OpenAIApiConfig value)
    {
        if (SelectedOpenAIApiConfigItem != null)
        {
            FilterAssistantsApiConfigItems();
        }
    }

    [ObservableProperty]
    private AssistantsApiConfigCollection _filteredAssistantsApiConfigItems;

    [ObservableProperty]
    private AvatarConfigCollection _avatarConfigItems;

    [ObservableProperty]
    private AvatarConfig _selectedAvatarConfigItem;

    [ObservableProperty]
    private string _assistantId = string.Empty;

    [ObservableProperty]
    private bool _retrieveAssistantButtonEnable = false;

    [ObservableProperty]
    private string _assistantName = string.Empty;

    [ObservableProperty]
    private string _model = string.Empty;

    [ObservableProperty]
    private List<string> _modelList = new List<string>();

    [ObservableProperty]
    private string _instructions = string.Empty;

    [ObservableProperty]
    private bool _toolFileSearch = false;
    partial void OnToolFileSearchChanged(bool oldValue, bool newValue)
    {
        AddVectorStoreButtonEnable = newValue;
    }

    [ObservableProperty]
    private bool _toolCodeInterpreter = false;

    [ObservableProperty]
    private double _temperature = 1;

    [ObservableProperty]
    private double _topP = 1;

    [ObservableProperty]
    private ToolResources _toolResources = new ToolResources();

    [ObservableProperty]
    private string _vectorStoreId = string.Empty;

    [ObservableProperty]
    private string _vectorStoreName = string.Empty;
    partial void OnVectorStoreNameChanged(string? oldValue, string newValue)
    {
        AddVectorStoreButtonEnable = String.IsNullOrEmpty(newValue);
    }

    [ObservableProperty]
    private FileObjects _codeInterpreterFileIds = new FileObjects();

    [ObservableProperty]
    private bool _codeInterpreterFileListVisible = false;

    [ObservableProperty]
    private bool _vectorStoreVisible = false;

    [ObservableProperty]
    private bool _isFlyoutOpen = false;

    [ObservableProperty]
    private bool _assistantInfoEnable = false;

    [ObservableProperty]
    private bool _createAssistantButtonEnable = false;

    [ObservableProperty]
    private bool _addVectorStoreButtonEnable = false;

    [ObservableProperty]
    private bool _isAssistantListProgressBarActive = false;

    [ObservableProperty]
    private bool _isAssistantProgressBarActive = false;

    public ConfigurationPageViewModel()
    {
        _assistantsApiConfigItems = App.Current.AssistantsApiConfigs;
        _openAIApiConfigItems = App.Current.OpenAIApiConfigs;
        //_avatarConfigItems = App.Current.AvatarConfigs ?? new AvatarConfigCollection();
    }

    [RelayCommand]
    private async Task OnAddButtonClick()
    {
        SelectedAssistantsApiItem = null;
        AssistantName = "";
        AssistantId = "";

        Instructions = "";
        Model = "";
        Temperature = 1;
        TopP = 1;
        ToolFileSearch = false;
        ToolCodeInterpreter = false;
        AssistantInfoEnable = true;
        CreateAssistantButtonEnable = true;
    }

    [RelayCommand]
    private async Task OnCreateAssistantButtonClick()
    {
        try
        {
            IsAssistantProgressBarActive = true;
            OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

            List<OpenAI.ObjectModels.RequestModels.ToolDefinition> tools = new List<OpenAI.ObjectModels.RequestModels.ToolDefinition>();
            if (ToolFileSearch)
            {
                tools.Add(new OpenAI.ObjectModels.RequestModels.ToolDefinition { Type = "file_search" });
            }
            if (ToolCodeInterpreter)
            {
                tools.Add(new OpenAI.ObjectModels.RequestModels.ToolDefinition { Type = "code_interpreter" });
            }

            AssistantCreateRequest createRequest = new AssistantCreateRequest
            {
                Name = AssistantName,
                Instructions = Instructions,
                Model = Model,
                Tools = tools,
                ToolResources = ToolResources,
                Temperature = (float?)Temperature,
                TopP = TopP,
            };

            var response = await openAiService.AssistantCreate(createRequest);

            if (response.Successful == true)
            {
                await MessageBoxHelper.ShowMessageAsync("Success", "Assistant created successfully.");
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
        finally
        {
            IsAssistantProgressBarActive = false;
            CreateAssistantButtonEnable = false;
        }
    }

    [RelayCommand]
    private async Task OnRetrieveAssistantButtonClick()
    {
        try
        {
            IsAssistantProgressBarActive = true;
            OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

            var listModels = (await openAiService.ListModel()).Models;
            foreach (var model in listModels)
            {
                if (model.Id.Contains("gpt-4")
                    || model.Id.Contains("gpt-3.5")
                    || model.Id.Contains("gpt-35")) // AOAI
                {
                    ModelList.Add(model.Id);
                }
            }
            ModelList.Sort();

            var assistant = await openAiService.AssistantRetrieve(SelectedAssistantsApiItem.AssistantId);
            AssistantName = assistant.Name;
            Instructions = assistant.Instructions;
            Model = assistant.Model;
            Temperature = assistant.Temperature;
            TopP = assistant.TopP;

            if (assistant.Tools != null)
            {
                foreach (var tool in assistant.Tools)
                {
                    if (tool.Type == "file_search")
                    {
                        ToolFileSearch = true;
                    }
                    if (tool.Type == "code_interpreter")
                    {
                        ToolCodeInterpreter = true;
                    }
                }
            }

            ToolResources = assistant.ToolResources;
            if (ToolResources.FileSearch != null)
            {
                if (ToolResources.FileSearch.VectorStoreIds.Count > 0)
                {
                    VectorStoreId = ToolResources.FileSearch.VectorStoreIds[0];
                    var vector = await openAiService.RetrieveVectorStore(VectorStoreId);
                    VectorStoreName = vector.Name;
                    VectorStoreVisible = true;
                }
            }
            else
            {
                VectorStoreVisible = false;
            }

            if (ToolResources.CodeInterpreter != null)
            {
                List<string> fileIds = ToolResources.CodeInterpreter.FileIds;
                foreach (var file in fileIds)
                {
                    var fileInfo = await openAiService.RetrieveFile(file);
                    CodeInterpreterFileIds.Add(new FileObject
                    {
                        FileId = fileInfo.Id,
                        FileName = fileInfo.FileName,
                    });
                }
                CodeInterpreterFileListVisible = true;
            }

            AssistantInfoEnable = true;
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
        finally
        {
            IsAssistantProgressBarActive = false;
        }
    }

    [RelayCommand]
    private async Task OnModifyAssistantButtonClick(object sender)
    {
        try
        {
            IsAssistantProgressBarActive = true;
            OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

            List<OpenAI.ObjectModels.RequestModels.ToolDefinition> tools = new List<OpenAI.ObjectModels.RequestModels.ToolDefinition>();
            if (ToolFileSearch)
            {
                tools.Add(new OpenAI.ObjectModels.RequestModels.ToolDefinition { Type = "file_search" });
            }
            if (ToolCodeInterpreter)
            {
                tools.Add(new OpenAI.ObjectModels.RequestModels.ToolDefinition { Type = "code_interpreter" });
            }

            AssistantModifyRequest modifyRequest = new AssistantModifyRequest
            {
                Name = AssistantName,
                Instructions = Instructions,
                Model = Model,
                Tools = tools,
                ToolResources = ToolResources,
                Temperature = (float?)Temperature,
                TopP = TopP,
            };

            AssistantResponse modify = await openAiService.AssistantModify(SelectedAssistantsApiItem.AssistantId, modifyRequest);

            if (modify.Successful == true)
            {
                await MessageBoxHelper.ShowMessageAsync("Success", "Modify Assistant succeeded.");
            }
            else
            {
                await MessageBoxHelper.ShowMessageAsync("Error", modify.Error.Message);
            }
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
            IsAssistantProgressBarActive = false;
        }
    }

    [RelayCommand]
    private async Task OnGetAssistantList()
    {
        try
        {
            IsAssistantListProgressBarActive = true;
            OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

            AssistantsApiConfigCollection allAssistants = new AssistantsApiConfigCollection();
            var assistants = (await openAiService.AssistantList(new PaginationRequest { Order = "asc" })).Data;
            foreach (var assistant in assistants)
            {
                allAssistants.Add(new AssistantsApiConfig
                {
                    AssistantId = assistant.Id,
                    AssistantName = assistant.Name,
                    ConfigurationName = SelectedOpenAIApiConfigItem.ConfigurationName
                });
            }

            IsAssistantListProgressBarActive = false;
            var dialogViewModel = new AssistantSelectionDialogViewModel(allAssistants, App.Current.AssistantsApiConfigs, SelectedOpenAIApiConfigItem.ConfigurationName);
            var dialog = new AssistantSelectionDialog(dialogViewModel);
            if (dialog.ShowDialog() == true)
            {
                var selectedAssistants = dialog.SelectedAssistants;
                var assistantsApiConfigsForSave = App.Current.AssistantsApiConfigs;

                var filteredExistingAssistants = App.Current.AssistantsApiConfigs.ToList()
                    .Where(a => a.ConfigurationName == SelectedOpenAIApiConfigItem.ConfigurationName)
                    .ToList();

                var assistantsToAdd = selectedAssistants
                    .Where(a => !filteredExistingAssistants.Any(e => e.AssistantId == a.AssistantId))
                    .Select(a => new AssistantsApiConfig
                    {
                        AssistantId = a.AssistantId,
                        AssistantName = a.AssistantName,
                        ConfigurationName = a.ConfigurationName
                    }).ToList();

                var assistantsToRemove = filteredExistingAssistants
                    .Where(e => !selectedAssistants.Any(a => a.AssistantId == e.AssistantId))
                    .ToList();

                var assistantsToUpdate = filteredExistingAssistants
                           .Where(e => selectedAssistants.Any(a => a.AssistantId == e.AssistantId && a.AssistantName != e.AssistantName))
                           .ToList();

                foreach (var assistant in assistantsToAdd)
                {
                    assistantsApiConfigsForSave.Add(assistant);
                }

                foreach (var assistant in assistantsToRemove)
                {
                    assistantsApiConfigsForSave.Remove(assistant);
                }

                foreach (var assistant in assistantsToUpdate)
                {
                    var updatedAssistant = selectedAssistants.First(a => a.AssistantId == assistant.AssistantId);
                    assistant.AssistantName = updatedAssistant.AssistantName;
                }

                App.Current.AssistantsApiConfigs = assistantsApiConfigsForSave;
            }

            AssistantsApiConfigItems = App.Current.AssistantsApiConfigs;

            FilterAssistantsApiConfigItems();
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
            IsAssistantListProgressBarActive = false;
        }
    }

    [RelayCommand]
    private async Task OnDeleteAssistantButtonClick()
    {
        try
        {
            IsAssistantListProgressBarActive = true;
            OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

            string message = $"Do you want to delete assistant?\r\n" +
                $"Assistant Name: {SelectedAssistantsApiItem.AssistantName}\r\n" +
                $"Assistant Id: {SelectedAssistantsApiItem.AssistantId}\r\n" +
                $"Configuration: {SelectedAssistantsApiItem.ConfigurationName}";
            Wpf.Ui.Controls.MessageBoxResult messageResult = await new Wpf.Ui.Controls.MessageBox
            {
                Title = "Confirm",
                Content = message,
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            }.ShowDialogAsync();

            if (messageResult != Wpf.Ui.Controls.MessageBoxResult.Primary)
            {
                return;
            }

            var response = await openAiService.AssistantDelete(SelectedAssistantsApiItem.AssistantId);

            if (response.Successful)
            {
                await MessageBoxHelper.ShowMessageAsync("Success", "Delete Assistant succeeded.");
            }
            else
            {
                throw new Exception(response.Error.Message);
            }
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
            IsAssistantListProgressBarActive = false;
        }
    }

    [RelayCommand]
    private async Task OnOpenVectorStore()
    {
        try
        {
            IsAssistantProgressBarActive = true;
            OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

            VectorStoreFileListRequest vectorRequest = new VectorStoreFileListRequest();

            var files = await openAiService.ListVectorStoreFiles(VectorStoreId, vectorRequest);
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

            var vectorStoreInfo = await openAiService.RetrieveVectorStore(VectorStoreId);

            IsAssistantProgressBarActive = false;
            var window = new VectorStoreInfo(vectorStoreInfo, vectorFileList, SelectedOpenAIApiConfigItem.ConfigurationName);
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
        finally
        {
            IsAssistantProgressBarActive = false;
        }
    }

    [RelayCommand]
    private async Task OnAddFileForFileSearchClick(object owner)
    {
        try
        {
            OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

            ObservableCollection<string> vectorStoreList = new ObservableCollection<string>();
            var response = await openAiService.ListVectorStores(new PaginationRequest { Order = "asc" });
            if (response.Successful == true)
            {
                foreach (var item in response.Data)
                {
                    vectorStoreList.Add(item.Name);
                }
            }
            else
            {
                throw new Exception(response.Error.Message);
            }

            var viewModel = new TextInputDialogViewModel("Please select Vector store:", vectorStoreList);
            var window = new TextInputDialog(viewModel);
            window.Owner = (Window)owner;
            window.ShowDialog();

            string vectorStoreName = string.Empty;
            if (window.DialogResult == true)
            {
                vectorStoreName = window.ResponseText;
            }
            else
            {
                return;
            }

            VectorStoreObjectResponse responseItem = response.Data.FirstOrDefault(x => x.Name == vectorStoreName);
            VectorStoreId = responseItem.Id;
            VectorStoreName = vectorStoreName;
            VectorStoreVisible = true;
            if (ToolResources.FileSearch == null)
            {
                ToolResources.FileSearch = new FileSearch();
            }
            if (ToolResources.FileSearch.VectorStoreIds == null)
            {
                ToolResources.FileSearch.VectorStoreIds = new List<string>();
            }
            ToolResources.FileSearch.VectorStoreIds.Add(VectorStoreId);
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
    }

    [RelayCommand]
    private void OnDetachVectorStoreButtonClick()
    {
        ToolResources.FileSearch = null;
        VectorStoreId = null;
        VectorStoreName = null;
        VectorStoreVisible = false;
        ToolFileSearch = false;
    }

    private void FilterAssistantsApiConfigItems()
    {
        if (SelectedOpenAIApiConfigItem != null)
        {
            string selectedConfigName = SelectedOpenAIApiConfigItem.ConfigurationName;
            var filteredList = AssistantsApiConfigItems.Where(item => item.ConfigurationName == selectedConfigName).ToList();
            FilteredAssistantsApiConfigItems = new AssistantsApiConfigCollection();
            foreach (var item in filteredList)
            {
                FilteredAssistantsApiConfigItems.Add(item);
            }
        }
        else
        {
            FilteredAssistantsApiConfigItems = new AssistantsApiConfigCollection();
            foreach (var item in AssistantsApiConfigItems)
            {
                FilteredAssistantsApiConfigItems.Add(item);
            }
        }
    }

    [RelayCommand]
    private async Task OnCodeInterpreterOpenFileInfo(object parameter)
    {
        try
        {
            string fileId = string.Empty;
            if (parameter is FileObject file)
            {
                fileId = file.FileId;
            }

            OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

            var fileResponse = await openAiService.RetrieveFile(fileId);

            if (fileResponse.Bytes != null)
            {
                await MessageBoxHelper.ShowMessageAsync("File", "FileName: " + fileResponse.FileName +
                    "\r\nStatus: " + fileResponse.Status +
                    "\r\nPurpose: " + fileResponse.Purpose +
                    "\r\nSize: " + $"{Math.Round((decimal)(fileResponse.Bytes / 1024f), 2).ToString("N2")} KB");
            }
            else
            {
                throw new InvalidOperationException("Failed to retrieve the file.");
            }
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
    }

    [RelayCommand]
    private async Task OnCodeInterpreterDeleteFile(object parameter)
    {
        try
        {
            if (parameter is FileObject file)
            {
                Wpf.Ui.Controls.MessageBoxResult messageResult = await new Wpf.Ui.Controls.MessageBox
                {
                    Title = "Confirm",
                    Content = $"Do you want to delete \"{file.FileName}\"?",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel"
                }.ShowDialogAsync();

                if (messageResult != Wpf.Ui.Controls.MessageBoxResult.Primary)
                {
                    return;
                }

                OpenAIService openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

                var deleteFileResponse = await openAiService.DeleteFile(file.FileId);

                if (deleteFileResponse.Successful == true)
                {
                    CodeInterpreterFileIds.Remove(file);

                    await MessageBoxHelper.ShowMessageAsync("Success", "File delete successfully.");

                    if (CodeInterpreterFileIds.Count == 0)
                    {
                        CodeInterpreterFileListVisible = false;
                    }
                }
                else
                {
                    await MessageBoxHelper.ShowMessageAsync("Error", deleteFileResponse.Error.Message);
                }
            }
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
    }
}
