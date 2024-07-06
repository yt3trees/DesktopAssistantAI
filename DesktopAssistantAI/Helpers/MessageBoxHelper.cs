namespace DesktopAssistantAI.Helpers;

public static class MessageBoxHelper
{
    public static async Task ShowMessageAsync(string title, string? content)
    {
        content ??= "An unknown error occurred.";

        Wpf.Ui.Controls.MessageBoxResult result = await new Wpf.Ui.Controls.MessageBox
        {
            Title = title,
            Content = content,
            CloseButtonText = "Close"
        }.ShowDialogAsync();
    }
}
