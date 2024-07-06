using DesktopAssistantAI.ViewModels.SubWindows;
using System.Windows.Input;

namespace DesktopAssistantAI.Views.SubWindows;

/// <summary>
/// CodeInterpreterWindow.xaml の相互作用ロジック
/// </summary>
public partial class CodeInterpreterWindow
{
    public CodeInterpreterWindow(string input, string output, byte[] imageFile)
    {
        InitializeComponent();
        DataContext = new CodeInterpreterWindowViewModel(input, output, imageFile);
        TextEditor.Text = ((CodeInterpreterWindowViewModel)DataContext).Input;
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
}
