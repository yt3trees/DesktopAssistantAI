using DesktopAssistantAI.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace DesktopAssistantAI.Views.Pages;

/// <summary>
/// StoragePage.xaml の相互作用ロジック
/// </summary>
public partial class StoragePage : INavigableView<StoragePageViewModel>
{
    public StoragePageViewModel ViewModel { get; }

    public StoragePage()
    {
        InitializeComponent();

        if (ConfigurationNameComboBox.Items.Count > 0)
        {
            ConfigurationNameComboBox.SelectedIndex = 0;
        }
    }
}
