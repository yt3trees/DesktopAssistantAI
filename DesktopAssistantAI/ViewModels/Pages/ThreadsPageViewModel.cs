using DesktopAssistantAI.Helpers;
using DesktopAssistantAI.Models;
using DesktopAssistantAI.Services;
using DesktopAssistantAI.Views.SubWindows;
using OpenAI.ObjectModels.SharedModels;
using System.Collections.ObjectModel;

namespace DesktopAssistantAI.ViewModels.Pages;

public partial class ThreadsPageViewModel : ObservableObject
{
    private const int PageSize = 7;

    [ObservableProperty]
    private OpenAIApiConfigCollection _openAIApiConfigItems;

    [ObservableProperty]
    private OpenAIApiConfig _selectedOpenAIApiConfigItem;
    partial void OnSelectedOpenAIApiConfigItemChanged(OpenAIApiConfig value)
    {
        if (SelectedOpenAIApiConfigItem != null)
        {
            FilterThreads();
        }
    }

    [ObservableProperty]
    private ThreadManagerCollection _threads;

    [ObservableProperty]
    private ThreadManagerCollection _filteredThreads = new ThreadManagerCollection();

    [ObservableProperty]
    private ThreadManagerCollection _currentPageThreads = new ThreadManagerCollection();

    [ObservableProperty]
    private int _currentPage = 1;

    public int TotalPages => (int)Math.Ceiling((double)FilteredThreads.Count / PageSize);

    public ThreadsPageViewModel()
    {
        _openAIApiConfigItems = App.Current.OpenAIApiConfigs;

        ThreadManagerService threadManagerService = new ThreadManagerService();
        _threads = threadManagerService.LoadThreadManagerCollection();
    }

    [RelayCommand]
    private async Task OnOpenThread(ThreadManager threadManager)
    {
        var threadId = threadManager.ThreadId;

        var openAiService = AssistantsApiService.CreateOpenAIService(SelectedOpenAIApiConfigItem.ConfigurationName);

        var afterRunMessagesResponse = await openAiService.ListMessages(threadId);
        var messages = afterRunMessagesResponse.Data;

        ObservableCollection<MessageResponse> sortedMessages = new ObservableCollection<MessageResponse>(
            (messages ?? new List<MessageResponse>()).OrderBy(m => m.CreatedAt)
        );

        var conversationWindow = new ConversationWindow(sortedMessages);
        conversationWindow.Show();
    }

    [RelayCommand]
    private async Task OnDeleteThread(ThreadManager threadManager)
    {
        if (threadManager == null)
        {
            return;
        }

        Wpf.Ui.Controls.MessageBoxResult result = await new Wpf.Ui.Controls.MessageBox
        {
            Title = "Confirm",
            Content = $"Do you want to delete \"{threadManager.ThreadId}\"?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel"
        }.ShowDialogAsync();

        if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
        {
            Threads.Remove(threadManager);
            FilteredThreads.Remove(threadManager);

            ThreadManagerService threadManagerService = new ThreadManagerService();
            threadManagerService.SaveThreadManagerCollection(Threads);

            UpdateCurrentPageThreads();
        }
    }

    [RelayCommand]
    private void OnThreadIdMouseLeftButtonUp(object thread)
    {
        if (thread is ThreadManager th)
        {
            Clipboard.SetText(th.ThreadId);
            MessageBoxHelper.ShowMessageAsync("Success", "ThreadId has been copied to the clipboard.");
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            UpdateCurrentPageThreads();
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            UpdateCurrentPageThreads();
        }
    }

    private void FilterThreads()
    {
        if (SelectedOpenAIApiConfigItem == null)
        {
            FilteredThreads = Threads;
        }
        else
        {
            var filtered = Threads
                                .Where(t => t.ConfigurationName == SelectedOpenAIApiConfigItem.ConfigurationName)
                                .OrderByDescending(t => t.CreatedAt)
                                .ToList();

            FilteredThreads.Clear();

            foreach (var thread in filtered)
            {
                FilteredThreads.Add(thread);
            }
        }
        CurrentPage = 1;
        UpdateCurrentPageThreads();
    }

    private void UpdateCurrentPageThreads()
    {
        var threadsToShow = FilteredThreads.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
        CurrentPageThreads.Clear();

        foreach (var thread in threadsToShow)
        {
            CurrentPageThreads.Add(thread);
        }

        OnPropertyChanged(nameof(TotalPages));
    }
}
