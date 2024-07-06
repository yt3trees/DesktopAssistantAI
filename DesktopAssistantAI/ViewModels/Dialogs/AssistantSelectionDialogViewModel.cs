using DesktopAssistantAI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopAssistantAI.ViewModels.Dialogs;

public partial class AssistantSelectionDialogViewModel : ObservableObject
{
    public ObservableCollection<AssistantSelectionItem> Assistants { get; set; } = new ObservableCollection<AssistantSelectionItem>();

    public AssistantSelectionDialogViewModel(IEnumerable<AssistantsApiConfig> allAssistants, IEnumerable<AssistantsApiConfig> existingAssistants, string configurationName)
    {
        var existingIds = new HashSet<string>(existingAssistants.Select(a => a.AssistantId));

        foreach (var assistant in allAssistants.Where(a => a.ConfigurationName == configurationName))
        {
            Assistants.Add(new AssistantSelectionItem
            {
                AssistantName = assistant.AssistantName,
                AssistantId = assistant.AssistantId,
                ConfigurationName = assistant.ConfigurationName,
                IsSelected = existingIds.Contains(assistant.AssistantId)
            });
        }
    }

    public class AssistantSelectionItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string AssistantName { get; set; }

        public string AssistantId { get; set; }

        public string ConfigurationName { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
