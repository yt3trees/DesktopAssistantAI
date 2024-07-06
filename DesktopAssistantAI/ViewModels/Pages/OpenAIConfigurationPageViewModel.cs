using DesktopAssistantAI.Models;
using System.Collections.ObjectModel;

namespace DesktopAssistantAI.ViewModels.Pages;

public partial class OpenAIConfigurationPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<OpenAIApiConfig> _openAIApiConfigItems;

    [ObservableProperty]
    private OpenAIApiConfig _selectedOpenAIApiConfigItem;

    partial void OnSelectedOpenAIApiConfigItemChanged(OpenAIApiConfig value)
    {
        if (SelectedOpenAIApiConfigItem != null)
        {
            ConfigurationName = SelectedOpenAIApiConfigItem.ConfigurationName;
            ApiKey = SelectedOpenAIApiConfigItem.ApiKey;
            Provider = SelectedOpenAIApiConfigItem.Provider;
            AzureResourceUrl = SelectedOpenAIApiConfigItem.AzureResourceUrl;
            AzureApiVersion = SelectedOpenAIApiConfigItem.AzureApiVersion;
            RadioButtonChoice = SelectedOpenAIApiConfigItem.Provider == "OpenAI" ? Choices.OpenAI : Choices.AzureOpenAI;
        }
    }

    [ObservableProperty]
    private string _configurationName = string.Empty;

    [ObservableProperty]
    private string _apiKey = string.Empty;

    [ObservableProperty]
    private string _provider = string.Empty;

    [ObservableProperty]
    private Choices _radioButtonChoice;

    partial void OnRadioButtonChoiceChanged(Choices value)
    {
        IsAzureOpenAI = (value == Choices.AzureOpenAI);
    }

    [ObservableProperty]
    private string _azureResourceUrl;

    [ObservableProperty]
    private string _azureApiVersion;

    [ObservableProperty]
    private bool _isAzureOpenAI;

    [ObservableProperty]
    private bool _isFlyoutOpen = false;

    public OpenAIConfigurationPageViewModel()
    {
        _openAIApiConfigItems = App.Current.OpenAIApiConfigs;
    }

    public enum Choices
    {
        OpenAI,
        AzureOpenAI
    };

    [RelayCommand]
    private async Task OnAddButtonClick()
    {
        SelectedOpenAIApiConfigItem = null;
        ConfigurationName = "";
        RadioButtonChoice = Choices.OpenAI;
        ApiKey = "";
        AzureResourceUrl = "";
        AzureApiVersion = "";
    }

    [RelayCommand]
    private async Task OnDeleteButtonClick(object sender)
    {
        if (SelectedOpenAIApiConfigItem != null)
        {
            string configurationName = SelectedOpenAIApiConfigItem.ConfigurationName;
            Wpf.Ui.Controls.MessageBoxResult result = await new Wpf.Ui.Controls.MessageBox
            {
                Title = "Confirm",
                Content = $"Do you want to delete \"{configurationName}\"?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            }.ShowDialogAsync();

            if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
            {
                OpenAIApiConfigItems.Remove(SelectedOpenAIApiConfigItem);
                SelectedOpenAIApiConfigItem = null;

                App.Current.OpenAIApiConfigs = (OpenAIApiConfigCollection)OpenAIApiConfigItems;

                if (OpenAIApiConfigItems.Count > 0)
                {
                    SelectedOpenAIApiConfigItem = OpenAIApiConfigItems[0];
                }
            }
            SelectedOpenAIApiConfigItem = null;
            ConfigurationName = "";
            RadioButtonChoice = Choices.OpenAI;
            ApiKey = "";
            AzureResourceUrl = "";
            AzureApiVersion = "";
        }
    }

    [RelayCommand]
    private void OnSaveButtonClick(object sender)
    {
        try
        {
            string choice = RadioButtonChoice == Choices.OpenAI ? "OpenAI" : "AzureOpenAI";

            if (SelectedOpenAIApiConfigItem == null)
            {
                var newItem = new OpenAIApiConfig
                {
                    ConfigurationName = ConfigurationName,
                    Provider = choice,
                    ApiKey = ApiKey,
                    AzureResourceUrl = AzureResourceUrl,
                    AzureApiVersion = AzureApiVersion,
                };
                OpenAIApiConfigItems.Add(newItem);
                SelectedOpenAIApiConfigItem = newItem;
            }
            else
            {
                int index = OpenAIApiConfigItems.IndexOf(SelectedOpenAIApiConfigItem);
                if (index != -1)
                {
                    OpenAIApiConfigItems[index] = new OpenAIApiConfig
                    {
                        ConfigurationName = ConfigurationName,
                        Provider = choice,
                        ApiKey = ApiKey,
                        AzureResourceUrl = AzureResourceUrl,
                        AzureApiVersion = AzureApiVersion,
                    };
                }
                SelectedOpenAIApiConfigItem = OpenAIApiConfigItems[index];
            }

            App.Current.OpenAIApiConfigs = (OpenAIApiConfigCollection)OpenAIApiConfigItems;

            if (!IsFlyoutOpen)
            {
                IsFlyoutOpen = true;
            }
        }
        catch (Exception ex)
        {
            new Wpf.Ui.Controls.MessageBox
            {
                Title = "Error",
                Content = ex.Message,
            }.ShowDialogAsync();
        }
    }
}
