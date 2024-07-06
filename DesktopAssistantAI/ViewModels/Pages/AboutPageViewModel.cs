using System.IO;
using System.Reflection;

namespace DesktopAssistantAI.ViewModels.Pages;

public partial class AboutPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _currentApplicationName = Assembly.GetExecutingAssembly().GetName().Name;

    [ObservableProperty]
    private string _appVersion = string.Empty;

    [ObservableProperty]
    private string _configurationPath = string.Empty;

    public AboutPageViewModel()
    {
        AppVersion = $"{Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty}";
        ConfigurationPath = Path.GetDirectoryName(System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);
    }
}
