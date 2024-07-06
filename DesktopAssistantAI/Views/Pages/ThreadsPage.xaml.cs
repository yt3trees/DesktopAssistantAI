using DesktopAssistantAI.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace DesktopAssistantAI.Views.Pages;

/// <summary>
/// ThreadsPage.xaml の相互作用ロジック
/// </summary>
public partial class ThreadsPage : INavigableView<ThreadsPageViewModel>
{
    public ThreadsPageViewModel ViewModel { get; }

    public ThreadsPage()
    {
        InitializeComponent();

        if (ConfigurationNameComboBox.Items.Count > 0)
        {
            ConfigurationNameComboBox.SelectedIndex = 0;
        }
    }
}
