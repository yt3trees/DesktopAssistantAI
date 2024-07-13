using DesktopAssistantAI.Helpers;
using DesktopAssistantAI.Models;
using DesktopAssistantAI.Services;
using DesktopAssistantAI.ViewModels.Dialogs;
using DesktopAssistantAI.Views.Dialogs;
using DesktopAssistantAI.Views.SubWindows;
using DesktopAssistantAI.Views.Windows;
using Markdig;
using Microsoft.Win32;
using Neo.Markdig.Xaml;
using OpenAI.Managers;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.SharedModels;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DesktopAssistantAI.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _applicationTitle = Assembly.GetExecutingAssembly().GetName().Name;

    private ThreadManagerService _threadManagerService = new ThreadManagerService();

    private ThreadManagerCollection _threadManagerCollection;

    private CancellationTokenSource _cts = new CancellationTokenSource();

    [ObservableProperty]
    private ObservableCollection<AssistantsApiConfig> _assistantsApiConfigs;

    [ObservableProperty]
    private AssistantsApiConfig _selectedAssistant;
    partial void OnSelectedAssistantChanged(AssistantsApiConfig value)
    {
        if (SelectedAssistant != null)
        {
            Properties.Settings.Default.SelectedAssistantId = SelectedAssistant.AssistantId;
            Properties.Settings.Default.Save();
        }
    }

    [ObservableProperty]
    private string _userInput = string.Empty;

    [ObservableProperty]
    private bool _userInputTextBoxVisible = false;
    partial void OnUserInputTextBoxVisibleChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            FileListVisible = Files.Any();
        }
        else
        {
            FileListVisible = false;
        }
    }

    [ObservableProperty]
    private bool _responseTextBoxVisible = false;

    [ObservableProperty]
    private bool _cancelButtonVisible = false;

    [ObservableProperty]
    private bool _progressRingVisible = false;

    [ObservableProperty]
    private bool _fileProgressRingVisible = false;

    //[ObservableProperty]
    //private double _responseTextBoxWidth = 300;

    [ObservableProperty]
    private double _responseTextBoxHeight = 600;

    [ObservableProperty]
    private double _responseTextBoxMaxHeight;

    [ObservableProperty]
    private double _gridMinHeight;

    [ObservableProperty]
    //private string _responseText = string.Empty;
    private System.Windows.Documents.FlowDocument _responseText = new System.Windows.Documents.FlowDocument();

    [ObservableProperty]
    private bool _sendButtonEnable = true;

    [ObservableProperty]
    private string _activeThreadId = string.Empty;

    [ObservableProperty]
    private string _activeRunId = string.Empty;

    [ObservableProperty]
    private FileObjects _files = new FileObjects();

    [ObservableProperty]
    private bool _isFlyoutOpen = false;

    [ObservableProperty]
    private bool _isAttachFlyoutOpen = false;

    [ObservableProperty]
    private bool _isUploadFlyoutOpen = false;

    [ObservableProperty]
    private bool _isImageUploadFlyoutOpen = false;

    [ObservableProperty]
    private string _activeImageUrl = string.Empty;

    [ObservableProperty]
    private bool _fileListVisible = false;

    [ObservableProperty]
    private bool _toolChoiceNone = false;
    partial void OnToolChoiceNoneChanged(bool oldValue, bool newValue)
    {
        if (newValue == true)
        {
            ToolChoiceFileSearch = false;
            ToolChoiceCodeInterpreter = false;
        }
    }

    [ObservableProperty]
    private bool _toolChoiceCodeInterpreter = false;
    partial void OnToolChoiceCodeInterpreterChanged(bool oldValue, bool newValue)
    {
        if (newValue == true)
        {
            ToolChoiceNone = false;
            ToolChoiceFileSearch = false;
        }
    }

    [ObservableProperty]
    private bool _toolChoiceFileSearch = false;
    partial void OnToolChoiceFileSearchChanged(bool oldValue, bool newValue)
    {
        if (newValue == true)
        {
            ToolChoiceNone = false;
            ToolChoiceCodeInterpreter = false;
        }
    }

    [ObservableProperty]
    private AvatarConfig _selectedAvatarConfigItem;

    [ObservableProperty]
    private double _avatarOpacity = 1;

    [ObservableProperty]
    private int _avatarSize = 150;

    public MainWindowViewModel()
    {
        var settings = DesktopAssistantAI.Properties.Settings.Default;

        double screenHeight = SystemParameters.WorkArea.Height;
        GridMinHeight = screenHeight * 2 / 3;
        ResponseTextBoxMaxHeight = GridMinHeight - 40;

        _assistantsApiConfigs = App.Current.AssistantsApiConfigs;
        var savedAssistantId = Properties.Settings.Default.SelectedAssistantId;
        if (!string.IsNullOrEmpty(savedAssistantId))
        {
            var savedItem = _assistantsApiConfigs.FirstOrDefault(item => item.AssistantId == savedAssistantId);
            if (savedItem != null)
            {
                SelectedAssistant = savedItem;
            }
        }

        _threadManagerCollection = _threadManagerService.LoadThreadManagerCollection();

        var avatarConfigItems = App.Current.AvatarConfigs ?? new AvatarConfigCollection();
        SelectedAvatarConfigItem = avatarConfigItems.FirstOrDefault(c => c.AvatarName == settings.SelectedAvatarConfig);

        if (string.IsNullOrEmpty(SelectedAvatarConfigItem.AvatarImagePath))
        {
            _ = MessageBoxHelper.ShowMessageAsync("Error", "AvatarImagePath is not set. Setting default value.");
            SelectedAvatarConfigItem.AvatarImagePath = "pack://application:,,,/Assets/OpenAI.png";
        }

        AvatarSize = AvatarControlHelper.SetAvatarSize(settings.AvatarSize);
    }

    [RelayCommand]
    private void OpenSettingsWindow(object owner)
    {
        try
        {
            var window = new SettingsWindow();
            window.ShowDialog();

            AssistantsApiConfigs = App.Current.AssistantsApiConfigs;
            var savedAssistantId = Properties.Settings.Default.SelectedAssistantId;
            if (!string.IsNullOrEmpty(savedAssistantId))
            {
                var savedItem = AssistantsApiConfigs.FirstOrDefault(item => item.AssistantId == savedAssistantId);
                if (savedItem != null)
                {
                    SelectedAssistant = savedItem;
                }
            }

            var settings = DesktopAssistantAI.Properties.Settings.Default;

            var avatarConfigItems = App.Current.AvatarConfigs ?? new AvatarConfigCollection();
            SelectedAvatarConfigItem = avatarConfigItems.FirstOrDefault(c => c.AvatarName == settings.SelectedAvatarConfig);

            AvatarSize = AvatarControlHelper.SetAvatarSize(settings.AvatarSize);
        }
        catch (Exception ex)
        {
            var result = new Wpf.Ui.Controls.MessageBox
            {
                Title = "Error",
                Content = ex.Message,
                CloseButtonText = "Close"
            }.ShowDialogAsync();
        }
    }

    [RelayCommand]
    private async Task OpenConversationReset(object owner)
    {
        ActiveThreadId = string.Empty;
        await OpenConversation(owner);
    }

    [RelayCommand]
    private async Task OpenConversation(object owner)
    {
        try
        {
            string activeThreadId = string.Empty;

            AssistantsApiConfig assistantConfig = AssistantsApiConfigManager.GetSelectedAssistantConfig();
            var openAiService = AssistantsApiService.CreateOpenAIService(assistantConfig.ConfigurationName);

            if (string.IsNullOrEmpty(ActiveThreadId))
            {
                var viewModel = new TextInputDialogViewModel("Please enter thread ID:");
                var window = new TextInputDialog(viewModel);
                window.Owner = (Window)owner;
                window.ShowDialog();

                if (window.DialogResult == true)
                {
                    activeThreadId = window.ResponseText;
                }
                else
                {
                    return;
                }
            }
            else
            {
                activeThreadId = ActiveThreadId;
            }

            var afterRunMessagesResponse = await openAiService.ListMessages(activeThreadId);
            var messages = afterRunMessagesResponse.Data;

            ObservableCollection<MessageResponse> sortedMessages = new ObservableCollection<MessageResponse>(
                (messages ?? new List<MessageResponse>()).OrderBy(m => m.CreatedAt)
            );

            var conversationWindow = new ConversationWindow(sortedMessages);
            conversationWindow.Owner = (Window)owner;
            conversationWindow.Show();

            ActiveThreadId = activeThreadId;
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
    }

    [RelayCommand]
    private void WindowClosing(object window)
    {
        if (window is Window w)
        {
            var settings = Properties.Settings.Default;
            settings.WindowMaximized = w.WindowState == WindowState.Maximized;
            w.WindowState = WindowState.Normal;
            settings.WindowLeft = w.Left;
            settings.WindowTop = w.Top;
            settings.Save();
        }
    }

    [RelayCommand]
    private async Task Send(object window)
    {
        string input = UserInput;

        InitializeUI();
        try
        {
            AssistantConfigMerger? assistantConfig = ConfigCombiner.MergeConfigBySelectedAssistantId();
            if (assistantConfig == null)
            {
                throw new Exception("not selected assistant config.");
            }

            OpenAIService openAiService = AssistantsApiService.CreateOpenAIServiceFromAssistantId(assistantConfig.AssistantId);

            AssistantResponse assistant = await openAiService.AssistantRetrieve(assistantConfig.AssistantId);

            if (string.IsNullOrEmpty(ActiveThreadId))
            {
                ThreadResponse thread = await openAiService.ThreadCreate();
                ActiveThreadId = thread.Id;

                var threadManager = new ThreadManager
                {
                    ThreadId = ActiveThreadId,
                    CreatedAt = thread.CreatedAt,
                    ConfigurationName = assistantConfig.ConfigurationName,
                    AssistantId = assistantConfig.AssistantId,
                    AssistantName = assistant.Name,
                    ResponseMessagePart = "None",
                };
                _threadManagerCollection.Add(threadManager);
                _threadManagerService.SaveThreadManager(threadManager);
            }

            // create message
            //byte[] binaryImage = await File.ReadAllBytesAsync(@"C:\work\image.jpg");
            byte[] binaryImage = null;
            string imageUrl = ActiveImageUrl;

            MessageContentOneOfType content = new MessageContentOneOfType();
            List<string>? fileIds = new List<string>(); // for AOAI

            if (assistantConfig.Provider == "OpenAI")
            {
                content.AsList = new List<OpenAI.ObjectModels.RequestModels.MessageContent>() { OpenAI.ObjectModels.RequestModels.MessageContent.TextContent(input) };
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    content.AsList?.Add(OpenAI.ObjectModels.RequestModels.MessageContent.ImageUrlContent(imageUrl));
                }
                if (binaryImage != null)
                {
                    content.AsList?.Add(OpenAI.ObjectModels.RequestModels.MessageContent.ImageBinaryContent(binaryImage, "png")); // API side is not supported.
                }
                foreach (var file in Files)
                {
                    if (file.Type == "vision")
                    {
                        content.AsList?.Add(OpenAI.ObjectModels.RequestModels.MessageContent.ImageFileContent(file.FileId, "auto"));
                    }
                }
            }
            else
            {
                content.AsString = input;
            }

            MessageCreateRequest request;
            List<Attachment> attachments = new List<Attachment>();
            foreach (var file in Files)
            {
                if (file.Type == "file_search")
                {
                    attachments.Add(new Attachment
                    {
                        FileId = file.FileId,
                        Tools = [OpenAI.ObjectModels.RequestModels.ToolDefinition.DefineFileSearch()]
                    });
                }
                else if (file.Type == "code_interpreter")
                {
                    attachments.Add(new Attachment
                    {
                        FileId = file.FileId,
                        Tools = [OpenAI.ObjectModels.RequestModels.ToolDefinition.DefineCodeInterpreter()]
                    });
                }
            }

            request = new MessageCreateRequest
            {
                Role = "user",
                Content = content,
                Attachments = attachments,
            };

            MessageResponse message = await openAiService.CreateMessage(ActiveThreadId, request);

            if (message.Successful == false)
            {
                throw new Exception(message.Error.Message);
            }

            ToolChoiceOneOfType toolChoice = new ToolChoiceOneOfType();
            List<ToolDefinition> toolDefinitions = new List<ToolDefinition>();
            if (ToolChoiceNone == true)
            {
                toolChoice = new ToolChoiceOneOfType("none");
                toolDefinitions = null;
            }
            else if (ToolChoiceFileSearch == true)
            {
                toolChoice = new ToolChoiceOneOfType { AsObject = new ToolChoice { Type = "file_search" } };
                toolDefinitions = [ToolDefinition.DefineFileSearch()];
            }
            else if (ToolChoiceCodeInterpreter == true)
            {
                toolChoice = new ToolChoiceOneOfType { AsObject = new ToolChoice { Type = "code_interpreter" } };
                toolDefinitions = [ToolDefinition.DefineCodeInterpreter()];
            }
            else
            {
                toolChoice = new ToolChoiceOneOfType("auto");
                toolDefinitions = null;
            }

            RunCreateRequest runCreateRequest = new RunCreateRequest
            {
                AssistantId = assistantConfig.AssistantId,
                ToolChoice = toolChoice,
                Tools = toolDefinitions,
                TruncationStrategy = new TruncationStrategy
                {
                    Type = "last_messages",
                    LastMessages = Properties.Settings.Default.LastMessages,
                }
            };

            var runResponse = openAiService.RunCreateAsStream(ActiveThreadId, runCreateRequest);

            ResponseText = new FlowDocument();
            string responseTextTotal = string.Empty;
            await foreach (var run in runResponse)
            {
                if (run.Successful)
                {
                    if (run is RunResponse rr)
                    {
                        ActiveRunId = rr.Id;
                    }
                    else if (run is MessageResponse messageResponse)
                    {
                        if (!string.IsNullOrEmpty(messageResponse.Id))
                        {
                            ResponseTextBoxVisible = true;
                            if (messageResponse.StreamEvent == "thread.message.delta")
                            {
                                responseTextTotal += messageResponse.Content?.FirstOrDefault()?.Text?.Value;
                                ResponseText = MarkdownXaml.ToFlowDocument(responseTextTotal,
                                           new MarkdownPipelineBuilder()
                                               .UseXamlSupportedExtensions()
                                               .UseSoftlineBreakAsHardlineBreak()
                                               .Build()
                                           );

                            }
                        }
                    }
                }
                else
                {
                    throw new Exception(run.Error.Message);
                }
            }

            var threadManagerToUpdate = _threadManagerCollection.FirstOrDefault(tm => tm.ThreadId == ActiveThreadId);
            if (threadManagerToUpdate != null && threadManagerToUpdate.ResponseMessagePart == "None")
            {
                threadManagerToUpdate.ResponseMessagePart = StringOperationHelper.TruncateString(responseTextTotal, 40);
                _threadManagerService.SaveThreadManager(threadManagerToUpdate);
            }
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
        finally
        {
            FinalizeUI();
        }
    }

    [RelayCommand]
    private async Task CancelSend()
    {
        AssistantConfigMerger? assistantConfig = ConfigCombiner.MergeConfigBySelectedAssistantId();
        if (assistantConfig == null)
        {
            throw new Exception("not selected assistant config.");
        }

        OpenAIService openAiService = AssistantsApiService.CreateOpenAIServiceFromAssistantId(assistantConfig.AssistantId);

        var result = await openAiService.RunCancel(ActiveThreadId, ActiveRunId);
        if (!result.Successful)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", result.Error.Message);
        }
    }

    private void InitializeUI()
    {
        UserInput = string.Empty;
        SendButtonEnable = false;
        CancelButtonVisible = true;
        ProgressRingVisible = true;
    }

    private void FinalizeUI()
    {
        SendButtonEnable = true;
        CancelButtonVisible = false;
        ProgressRingVisible = false;
        ActiveRunId = string.Empty;
    }

    [RelayCommand]
    private async Task MouseEnter()
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        UserInputTextBoxVisible = true;
        TextRange textRange = new TextRange(ResponseText.ContentStart, ResponseText.ContentEnd);
        ResponseTextBoxVisible = textRange.Text != "" ? true : false;
        AvatarOpacity = 1;
    }

    [RelayCommand]
    private async Task MouseLeave()
    {
        try
        {
            await Task.Delay(2000, _cts.Token);
            if (!UserInputTextFocus && UserInput == "" && SendButtonEnable)
            {
                UserInputTextBoxVisible = false;
                AvatarOpacity = Properties.Settings.Default.AvatarOpacity;
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    [RelayCommand]
    private async Task WindowDeactivated()
    {
        UserInputTextFocus = false;
        if (UserInput == "" && !ProgressRingVisible)
        {
            UserInputTextBoxVisible = false;
            AvatarOpacity = Properties.Settings.Default.AvatarOpacity;
        }
        //if (ResponseText == "")
        //{
        ResponseTextBoxVisible = false;
        //}
    }

    [ObservableProperty]
    private bool _userInputTextFocus = false;

    [RelayCommand]
    private async Task UserInputTextBox_GotFocus()
    {
        UserInputTextFocus = true;
    }

    [RelayCommand]
    private async Task UserInputTextBox_LostFocus()
    {
        UserInputTextFocus = false;
    }

    [RelayCommand]
    private async Task ClearActiveThread(MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ActiveThreadId = string.Empty;
            ResponseText = new FlowDocument();
            ResponseTextBoxVisible = false;
            Files = new FileObjects();

            if (!IsFlyoutOpen)
            {
                await Task.Delay(1);
                IsFlyoutOpen = true;
            }
        }
    }

    [RelayCommand]
    private void AttachFlyoutOpen(object sender)
    {
        IsAttachFlyoutOpen = false;
        IsAttachFlyoutOpen = true;
    }

    [RelayCommand]
    private void UploadFlyoutOpen(object sender)
    {
        IsUploadFlyoutOpen = false;
        IsUploadFlyoutOpen = true;
    }

    [RelayCommand]
    private void ImageUploadFlyoutOpen(object sender)
    {
        IsImageUploadFlyoutOpen = false;
        IsImageUploadFlyoutOpen = true;
    }

    [RelayCommand]
    private async Task OnUploadImageAndAttach(string content)
    {
        FileListVisible = true;

        string type = string.Empty;
        string purpose = string.Empty;
        if (content == "File Search")
        {
            type = "file_search";
            purpose = "assistants";
        }
        else if (content == "Code Interpreter")
        {
            type = "code_interpreter";
            purpose = "assistants";
        }
        else if (content == "Select file" || content == "Paste from clipboard")
        {
            type = "vision";
            purpose = "vision";
        }
        else
        {
            return;
        }

        string fileName = string.Empty;
        byte[] fileBytes = [];
        if (content != "Paste from clipboard")
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
            fileName = Path.GetFileName(openFileDialog.FileName);
            fileBytes = File.ReadAllBytes(openFileDialog.FileName);
        }
        else
        {
            if (Clipboard.ContainsImage())
            {
                var image = Clipboard.GetImage();
                using (var memoryStream = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(memoryStream);
                    fileBytes = memoryStream.ToArray();
                    fileName = "clipboard.png";
                }
            }
            else
            {
                await MessageBoxHelper.ShowMessageAsync("Error", "The clipboard does not contain any images.");
            }
        }

        try
        {
            FileProgressRingVisible = true;
            AssistantsApiConfig assistantConfig = AssistantsApiConfigManager.GetSelectedAssistantConfig();
            var openAiService = AssistantsApiService.CreateOpenAIService(assistantConfig.ConfigurationName);

            var fileUploadResponse = await openAiService.UploadFile(purpose, fileBytes, fileName);
            FileProgressRingVisible = false;

            if (fileUploadResponse.Successful == true)
            {
                await MessageBoxHelper.ShowMessageAsync("Success", "File added successfully.");

                Files.Add(new FileObject
                {
                    FileId = fileUploadResponse.Id,
                    FileName = fileUploadResponse.FileName,
                    Uploaded = fileUploadResponse.CreatedAt,
                    Type = type,
                });
            }
            else
            {
                await MessageBoxHelper.ShowMessageAsync("Error", fileUploadResponse.Error.Message);
            }
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
        finally
        {
            FileProgressRingVisible = false;
        }
    }

    [RelayCommand]
    private async Task OnSetImageFromUrl(object owner)
    {
        if (!string.IsNullOrEmpty(ActiveImageUrl))
        {
            Wpf.Ui.Controls.MessageBoxResult result = await new Wpf.Ui.Controls.MessageBox
            {
                Title = "Confirm",
                Content = $"ImageUrl is already set. Can I overwrite it?\r\nCurrent set: {ActiveImageUrl}",
                PrimaryButtonText = "OK",
                CloseButtonText = "Close"
            }.ShowDialogAsync();

            if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
            {
                ActiveImageUrl = string.Empty;
            }
            else
            {
                return;
            }
        }

        var viewModel = new TextInputDialogViewModel("Set image from url:");
        var window = new TextInputDialog(viewModel);
        window.Owner = (Window)owner;
        window.ShowDialog();

        if (window.DialogResult == true)
        {
            ActiveImageUrl = window.ResponseText;
        }
    }

    [RelayCommand]
    private async Task OnRemoveAttachment(FileObject attachment)
    {
        if (attachment == null) return;

        Files.Remove(attachment);
        FileListVisible = Files.Any();
    }
}
