using System.Windows.Input;
using DesktopAssistantAI.ViewModels.Dialogs;

namespace DesktopAssistantAI.Views.Dialogs;

/// <summary>
/// TextInputDialog.xaml の相互作用ロジック
/// </summary>
public partial class TextInputDialog
{
    public string ResponseText { get; private set; }

    public TextInputDialog(TextInputDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        if (((TextInputDialogViewModel)DataContext).IsInputTextBoxVisible)
        {
            InputTextBox.Focus();
        }
        else if (((TextInputDialogViewModel)DataContext).IsInputComboBoxVisible)
        {
            InputComboBox.Focus();
        }
        else if (((TextInputDialogViewModel)DataContext).IsSliderVisible)
        {
            InputSlider.Focus();
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        var dataContext = (TextInputDialogViewModel)DataContext;

        if (dataContext.IsInputTextBoxVisible == true)
        {
            ResponseText = dataContext.InputTextBoxText;
        }
        else if (dataContext.IsInputComboBoxVisible == true)
        {
            ResponseText = dataContext.InputComboBoxText;
        }
        else if (dataContext.IsInputTextBoxVisible == false)
        {
            ResponseText = dataContext.InputSliderValue.ToString();
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void FluentWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
        }
    }
}
