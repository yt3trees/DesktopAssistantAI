using DesktopAssistantAI.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace DesktopAssistantAI.Views.Pages;

/// <summary>
/// AboutPage.xaml の相互作用ロジック
/// </summary>
public partial class AboutPage : INavigableView<AboutPageViewModel>
{
    public AboutPageViewModel ViewModel { get; }

    public AboutPage()
    {
        InitializeComponent();
    }
}
