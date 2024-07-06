using DesktopAssistantAI.Helpers;
using DesktopAssistantAI.Models;
using DesktopAssistantAI.ViewModels.Dialogs;
using DesktopAssistantAI.Views.Dialogs;
using System.Collections.ObjectModel;

namespace DesktopAssistantAI.ViewModels.Pages;

public partial class OptionsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private int _lastMessages = 0;

    [ObservableProperty]
    private AvatarConfigCollection _avatarConfigItems;

    [ObservableProperty]
    private AvatarConfig _selectedAvatarConfigItem;

    [ObservableProperty]
    private double _avatarOpacity = 100;

    [ObservableProperty]
    private bool _isFlyoutOpen = false;

    [ObservableProperty]
    private ObservableCollection<string> _avatarSizeItemSource = AvatarControlHelper.GetAvatarSizeOptions();

    [ObservableProperty]
    private string _avatarSize = string.Empty;

    public OptionsPageViewModel()
    {
        var settings = DesktopAssistantAI.Properties.Settings.Default;

        AvatarConfigItems = App.Current.AvatarConfigs ?? new AvatarConfigCollection();
        SelectedAvatarConfigItem = AvatarConfigItems.FirstOrDefault(c => c.AvatarName == settings.SelectedAvatarConfig);

        LastMessages = settings.LastMessages;
        AvatarOpacity = settings.AvatarOpacity * 100;
        AvatarSize = settings.AvatarSize;
    }

    [RelayCommand]
    private async Task Save()
    {
        var settings = DesktopAssistantAI.Properties.Settings.Default;

        settings.SelectedAvatarConfig = SelectedAvatarConfigItem.AvatarName;
        settings.LastMessages = LastMessages;
        settings.AvatarOpacity = AvatarOpacity / 100;
        settings.AvatarSize = AvatarSize;
        settings.Save();

        if (!IsFlyoutOpen)
        {
            await Task.Delay(1);
            IsFlyoutOpen = true;
        }
    }

    [RelayCommand]
    private async Task AddAvatarImage(object owner)
    {
        try
        {
            string imageName = string.Empty;

            var manager = new AvatarManagerService();
            string? fileName = manager.SetFile();
            if (fileName == null) { return; }

            if (manager.Successful)
            {
                var viewModel = new TextInputDialogViewModel("Avatar image name:");
                var window = new TextInputDialog(viewModel);
                window.Owner = (Window)owner;
                window.ShowDialog();
                if (window.DialogResult == true)
                {
                    imageName = window.ResponseText;
                }

                App.Current.AvatarConfigs.Add(new AvatarConfig
                {
                    AvatarName = imageName,
                    DisplayType = "Image",
                    AvatarImagePath = fileName,
                });
            }
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
    }

    [RelayCommand]
    private async Task DeleteAvatarImage(object owner)
    {
        try
        {
            string avatarName = SelectedAvatarConfigItem.AvatarName;
            //var manager = new AvatarManagerService();
            //manager.DeleteFile(SelectedAvatarConfigItem);

            App.Current.AvatarConfigs.Remove(SelectedAvatarConfigItem);

            //if (manager.Successful)
            //{
            await MessageBoxHelper.ShowMessageAsync("Successful", $"Delete {avatarName}.");
            //}
        }
        catch (Exception ex)
        {
            await MessageBoxHelper.ShowMessageAsync("Error", ex.Message);
        }
    }
}

