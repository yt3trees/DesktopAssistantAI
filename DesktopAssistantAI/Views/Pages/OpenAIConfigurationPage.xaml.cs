using DesktopAssistantAI.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace DesktopAssistantAI.Views.Pages;

/// <summary>
/// ConfigurationPage.xaml の相互作用ロジック
/// </summary>
public partial class OpenAIConfigurationPage : INavigableView<OpenAIConfigurationPageViewModel>
{
    public OpenAIConfigurationPageViewModel ViewModel { get; }

    public OpenAIConfigurationPage()
    {
        InitializeComponent();
        if (ConfigurationNameComboBox.Items.Count > 0)
        {
            ConfigurationNameComboBox.SelectedIndex = 0;
        }
    }
    public OpenAIConfigurationPage(OpenAIConfigurationPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
