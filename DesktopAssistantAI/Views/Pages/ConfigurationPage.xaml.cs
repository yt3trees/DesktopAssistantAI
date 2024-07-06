using DesktopAssistantAI.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace DesktopAssistantAI.Views.Pages;

/// <summary>
/// ConfigurationPage.xaml の相互作用ロジック
/// </summary>
public partial class ConfigurationPage : INavigableView<ConfigurationPageViewModel>
{
    public ConfigurationPageViewModel ViewModel { get; }

    public ConfigurationPage()
    {
        InitializeComponent();

        if (ConfigurationNameComboBox.Items.Count > 0)
        {
            ConfigurationNameComboBox.SelectedIndex = 0;
        }
    }

    public ConfigurationPage(ConfigurationPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
