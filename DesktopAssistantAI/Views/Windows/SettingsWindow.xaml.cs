using DesktopAssistantAI.ViewModels;
using DesktopAssistantAI.Views.Pages;

namespace DesktopAssistantAI.Views.Windows;

/// <summary>
/// Settings.xaml の相互作用ロジック
/// </summary>
public partial class SettingsWindow
{
    public SettingsWindowViewModel ViewModel { get; }

    //public SettingsWindow(SettingsWindowViewModel viewModel)
    //{
    //    ViewModel = viewModel;
    //    DataContext = this;
    //    InitializeComponent();
    //}

    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not SettingsWindow window)
        {
            return;
        }

        _ = window.NavigationView.Navigate(typeof(OpenAIConfigurationPage));
    }
}
