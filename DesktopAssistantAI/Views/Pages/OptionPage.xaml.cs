using DesktopAssistantAI.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace DesktopAssistantAI.Views.Pages;

/// <summary>
/// OptionsPage.xaml の相互作用ロジック
/// </summary>
public partial class OptionsPage : INavigableView<OptionsPageViewModel>
{
    public OptionsPageViewModel ViewModel { get; }

    public OptionsPage()
    {
        InitializeComponent();
    }
}
