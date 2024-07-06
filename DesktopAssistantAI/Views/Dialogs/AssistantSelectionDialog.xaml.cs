using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using DesktopAssistantAI.ViewModels.Dialogs;
using static DesktopAssistantAI.ViewModels.Dialogs.AssistantSelectionDialogViewModel;

namespace DesktopAssistantAI.Views.Dialogs;

/// <summary>
/// AssistantSelectionDialog.xaml の相互作用ロジック
/// </summary>
public partial class AssistantSelectionDialog
{
    public ObservableCollection<AssistantSelectionItem> SelectedAssistants { get; private set; }

    public AssistantSelectionDialog(AssistantSelectionDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        AssistantListBox.ItemsSource = viewModel.Assistants;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedAssistants = new ObservableCollection<AssistantSelectionItem>(((AssistantSelectionDialogViewModel)DataContext).Assistants.Where(a => a.IsSelected));
        DialogResult = true;
        Close();
    }

    private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBlock textBlock && textBlock.DataContext is AssistantSelectionItem item)
        {
            item.IsSelected = !item.IsSelected;
        }
        if (sender is Label label && label.DataContext is AssistantSelectionItem itemLabel)
        {
            itemLabel.IsSelected = !itemLabel.IsSelected;
        }
    }
}
