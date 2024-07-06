using AutoMapper;
using DesktopAssistantAI.Helpers;
using DesktopAssistantAI.Services;
using DesktopAssistantAI.Views.SubWindows;
using Markdig;
using Microsoft.Win32;
using Neo.Markdig.Xaml;
using OpenAI.Managers;
using OpenAI.ObjectModels.SharedModels;
using System.Collections.ObjectModel;
using System.IO;
using static OpenAI.ObjectModels.SharedModels.MessageResponse;

namespace DesktopAssistantAI.ViewModels.SubWindows;

public partial class ConversationWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _assistantId = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MessageResponseAdd> _messages = new ObservableCollection<MessageResponseAdd>();

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private bool _isProgressBarActive = false;

    public ConversationWindowViewModel(ObservableCollection<MessageResponse> messages)
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
        var mapper = config.CreateMapper();

        foreach (var message in messages)
        {
            MessageResponseAdd messageAdd = mapper.Map<MessageResponseAdd>(message);
            Messages.Add(messageAdd);
        }

        foreach (var message in Messages)
        {
            if (message.AssistantId != null)
            {
                AssistantId = message.AssistantId;
                break;
            }
        }

        Title = messages[0].ThreadId;

        _ = GetRunInfo();
    }

    public ConversationWindowViewModel()
    {
    }

    [RelayCommand]
    private async Task OnOpenFileInfo(object param)
    {
        try
        {
            string fileId = string.Empty;

            if (param is OpenAI.ObjectModels.RequestModels.Attachment attachment)
            {
                fileId = attachment.FileId;
            }
            else if (param is MessageContentResponse content)
            {
                fileId = content.ImageFile.FileId;
            }

            OpenAIService openAiService = AssistantsApiService.CreateOpenAIServiceFromAssistantId(AssistantId);

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
    private async Task OnGetCode(object param)
    {
        try
        {
            if (param is MessageResponseAdd res)
            {
                if (res.CodeInput == null)
                {
                    await MessageBoxHelper.ShowMessageAsync("Info", "Tool is not set.");
                    return;
                }

                OpenAIService openAiService = AssistantsApiService.CreateOpenAIServiceFromAssistantId(AssistantId);

                var input = res.CodeInput;
                string output = res.CodeOutput;
                string fileId = res.FileId;

                byte[] imageFile = [];
                if (!string.IsNullOrEmpty(fileId))
                {
                    OpenAI.ObjectModels.ResponseModels.FileResponseModels.FileContentResponse<byte[]> imageResponse = await openAiService.RetrieveFileContent<byte[]>(fileId);
                    imageFile = imageResponse.Content;
                }


                var window = new CodeInterpreterWindow(input, output, imageFile);
                window.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
    }
    [RelayCommand]
    private async Task OnGetInstructions(object instructions)
    {
        try
        {
            if (instructions is string instructionsValue)
            {
                await MessageBoxHelper.ShowMessageAsync("Instructions", instructionsValue);
            }
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
    }

    [RelayCommand]
    private async Task OnFileDownload(object param)
    {
        try
        {
            if (param is MessageResponse res)
            {
                string annotation = res.Content[0].Text.Annotations[0].Text;
                string fileName = Path.GetFileName(annotation);

                OpenAIService openAiService = AssistantsApiService.CreateOpenAIServiceFromAssistantId(AssistantId);

                string fileId = res.Attachments[0].FileId;
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

                    File.WriteAllBytes(openFileDialog.FileName, file);

                    await MessageBoxHelper.ShowMessageAsync("Success", $"Save to {openFileDialog.FileName}");
                }
            }
            else if (param is MessageContentResponse mcr)
            {
                OpenAIService openAiService = AssistantsApiService.CreateOpenAIServiceFromAssistantId(AssistantId);
                OpenAI.ObjectModels.ResponseModels.FileResponseModels.FileContentResponse<byte[]> response = await openAiService.RetrieveFileContent<byte[]>(mcr.ImageFile.FileId);

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

                    openFileDialog.FileName = "image_file.png";

                    if (openFileDialog.ShowDialog() != true)
                    {
                        return;
                    }

                    File.WriteAllBytes(openFileDialog.FileName, file);

                    await MessageBoxHelper.ShowMessageAsync("Success", $"Save to {openFileDialog.FileName}");
                }
            }
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
    }

    private async Task GetRunInfo()
    {
        try
        {
            IsProgressBarActive = true;

            OpenAIService openAiService = AssistantsApiService.CreateOpenAIServiceFromAssistantId(AssistantId);

            int runStepCount = 0;
            string processingRunId = string.Empty;

            foreach (var message in Messages)
            {
                if (message.RunId == null) continue;

                if (processingRunId != message.RunId)
                {
                    runStepCount = 0;
                }

                var response = await openAiService.RunRetrieve(message.ThreadId, message.RunId);
                if (!response.Successful)
                {
                    throw new Exception(response.Error.Message);
                }

                processingRunId = message.RunId;
                var runStepResponse = await openAiService.RunStepsList(message.ThreadId, message.RunId, new OpenAI.ObjectModels.RequestModels.PaginationRequest { Order = "asc" });
                if (!runStepResponse.Successful)
                {
                    throw new Exception(runStepResponse.Error.Message);
                }

                if (response.Tools != null)
                {
                    foreach (var tool in response.Tools)
                    {
                        if (tool.Type == "code_interpreter")
                        {
                            //message.IsCodeInterpreter = true;
                        }
                        else if (tool.Type == "file_search")
                        {
                            //message.IsFileSearch = true;
                        }
                    }
                }

                if (runStepResponse.Data == null || runStepResponse.Data.Count == 0) continue;

                int tempCount = 0;
                for (int i = runStepCount; i < runStepResponse.Data.Count; i++)
                {
                    var stepDetails = runStepResponse.Data[i].StepDetails;
                    if (stepDetails == null || stepDetails.toolCalls == null || stepDetails.toolCalls[0].CodeInterpreter == null)
                    {
                        runStepCount++;
                        continue;
                    }

                    if (stepDetails.Type != "message_creation")
                    {
                        var codeInterpreter = stepDetails.toolCalls[0].CodeInterpreter;
                        message.IsCodeInterpreter = true;
                        message.CodeInput = codeInterpreter.Input;
                        if (codeInterpreter.Outputs.Count > 0)
                        {
                            var output = codeInterpreter.Outputs[0];
                            message.CodeOutput = output.Logs;
                            if (output.Image != null)
                            {
                                message.FileId = output.Image.FileId;
                            }
                        }
                        else
                        {
                            message.CodeOutput = "Output log is Empty.";
                        }
                        break;
                    }

                    runStepCount++;
                }

                runStepCount++;

                if (response.Usage != null)
                {
                    message.TotalTokens = response.Usage.TotalTokens;
                    message.CompletionTokens = response.Usage.CompletionTokens;
                    message.PromptTokens = response.Usage.PromptTokens;
                }

                message.Instructions = response.Instructions;

                if (message.Content[0].Text.Annotations.Count > 0)
                {
                    message.IsFileSearch = true;
                    foreach (var annotation in message.Content[0].Text.Annotations)
                    {
                        var fileInformation = await openAiService.RetrieveFile(annotation.FileCitation.FileId);
                        if (message.AnnotationsList == null)
                        {
                            message.AnnotationsList = new List<AnnotationsList>();
                        };
                        message.AnnotationsList.Add(new AnnotationsList
                        {
                            Annotation = annotation.Text,
                            FileId = annotation.FileCitation.FileId,
                            FileName = fileInformation.FileName,
                        });
                    }

                    message.AnnotationsList = message.AnnotationsList
                                .Distinct()
                                .OrderBy(a => a.Annotation)
                                .ToList();
                }
            }
            var mes = Messages;
            Messages = null;
            Messages = mes;
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

    public record MessageResponseAdd : OpenAI.ObjectModels.SharedModels.MessageResponse
    {
        public bool IsCodeInterpreter { get; set; }

        public bool IsFileSearch { get; set; }

        public int TotalTokens { get; set; }

        public int? CompletionTokens { get; set; }

        public int PromptTokens { get; set; }

        public string? Instructions { get; set; }

        public string? CodeInput { get; set; }

        public string? CodeOutput { get; set; }

        public string? FileId { get; set; }

        public List<MessageContentResponseAdd>? FlowDocumentContent => this.Content?.Select(content => new MessageContentResponseAdd(content)).ToList();

        public List<AnnotationsList>? AnnotationsList { get; set; }
    }

    public record MessageContentResponseAdd : MessageContentResponse
    {
        public MessageTextAdd? FlowDocumentText { get; set; }

        public MessageContentResponseAdd(MessageContentResponse content)
        {
            Type = content.Type;
            Text = content.Text;
            ImageFile = content.ImageFile;
            ImageUrl = content.ImageUrl;

            if (content.Text != null)
            {
                FlowDocumentText = new MessageTextAdd { ValueAdd = content.Text.Value };
            }
        }
    }

    public record MessageTextAdd : MessageText
    {
        public string? ValueAdd;

        public System.Windows.Documents.FlowDocument FlowDocumentValue => MarkdownXaml.ToFlowDocument(this.ValueAdd,
            new MarkdownPipelineBuilder()
                .UseXamlSupportedExtensions()
                .UseSoftlineBreakAsHardlineBreak()
                .Build()
            );
    }

    public record AnnotationsList
    {
        public string Annotation { get; set; }

        public string FileId { get; set; }

        public string FileName { get; set; }

        bool IEquatable<AnnotationsList>.Equals(AnnotationsList other)
        {
            if (other == null) return false;
            return Annotation == other.Annotation;
        }

        public override int GetHashCode()
        {
            return Annotation != null ? Annotation.GetHashCode() : 0;
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<MessageResponse, MessageResponseAdd>();
        }
    }
}
