using DesktopAssistantAI.ViewModels.SubWindows;
using OpenAI.ObjectModels.SharedModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DesktopAssistantAI.Views.SubWindows;

/// <summary>
/// ConversationWindow.xaml の相互作用ロジック
/// </summary>
public partial class ConversationWindow
{
    public ConversationWindow(ObservableCollection<MessageResponse> messages)
    {
        InitializeComponent();
        DataContext = new ConversationWindowViewModel(messages);
    }

    private void FluentWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // スクロールイベントを無視して親にバブルさせる
        e.Handled = true;

        // 親のスクロールイベントを処理するために再度RaiseEventする
        var parent = (UIElement)((FrameworkElement)sender).Parent;
        parent.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = UIElement.MouseWheelEvent
        });
    }

    private async void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        string url = $"{e.Parameter}";
        var result = await new Wpf.Ui.Controls.MessageBox
        {
            Title = "Confirm",
            Content = $"Open link?\r\n{url}",
            PrimaryButtonText = "OK",
            CloseButtonText = "Close"
        }.ShowDialogAsync();

        if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
        {
            ProcessStartInfo pi = new ProcessStartInfo()
            {
                FileName = url,
                UseShellExecute = true,
            };
            Process.Start(pi);
        }
    }
}
