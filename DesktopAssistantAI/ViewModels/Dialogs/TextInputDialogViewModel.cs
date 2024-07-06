using System.Collections.ObjectModel;

namespace DesktopAssistantAI.ViewModels.Dialogs;

public partial class TextInputDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _textContent = string.Empty;

    [ObservableProperty]
    private bool _isInputTextBoxVisible = false;

    [ObservableProperty]
    private bool _isInputComboBoxVisible = false;

    [ObservableProperty]
    private bool _isSliderVisible = false;

    [ObservableProperty]
    private string _inputTextBoxText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _comboBoxItem = new ObservableCollection<string>();

    [ObservableProperty]
    private string _inputComboBoxText = string.Empty;

    [ObservableProperty]
    private double _inputSliderValue = 0;

    [ObservableProperty]
    private double _inputSliderMaximum = 0;

    [ObservableProperty]
    private double _inputSliderMinimum = 0;

    [ObservableProperty]
    private double _inputSliderTickFrequency = 0;

    public TextInputDialogViewModel(string content)
    {
        TextContent = content;
        IsInputTextBoxVisible = true;
        IsInputComboBoxVisible = false;
        IsSliderVisible = false;
    }

    public TextInputDialogViewModel(string content, ObservableCollection<string> list)
    {
        TextContent = content;
        IsInputTextBoxVisible = false;
        IsInputComboBoxVisible = true;
        IsSliderVisible = false;
        ComboBoxItem = list;
    }

    public TextInputDialogViewModel(string content, double maximum, double minimum, double tickFrequency)
    {
        TextContent = content;
        IsInputTextBoxVisible = false;
        IsInputComboBoxVisible = false;
        IsSliderVisible = true;
        InputSliderMaximum = maximum;
        InputSliderMinimum = minimum;
        InputSliderTickFrequency = tickFrequency;
    }
}
