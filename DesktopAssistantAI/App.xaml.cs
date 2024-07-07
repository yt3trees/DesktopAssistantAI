using DesktopAssistantAI.Helpers;
using DesktopAssistantAI.Models;
using DesktopAssistantAI.Services;
using DesktopAssistantAI.ViewModels;
using DesktopAssistantAI.ViewModels.Pages;
using DesktopAssistantAI.ViewModels.SubWindows;
using DesktopAssistantAI.ViewModels.Windows;
using DesktopAssistantAI.Views.Pages;
using DesktopAssistantAI.Views.SubWindows;
using DesktopAssistantAI.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Wpf.Ui;

namespace DesktopAssistantAI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static new App Current => (App)Application.Current;

        private static Mutex mutex;

        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)); })
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<ApplicationHostService>();

                // Page resolver service
                services.AddSingleton<IPageService, PageService>();

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // Main window with navigation
                services.AddSingleton<INavigationWindow, MainWindow>();
                //services.AddSingleton<MainWindow>();
                services.AddSingleton<MainWindowViewModel>();

                services.AddSingleton<IContentDialogService, ContentDialogService>();

                services.AddSingleton<SettingsWindow>();
                services.AddSingleton<SettingsWindowViewModel>();
                services.AddSingleton<ConfigurationPage>();
                services.AddSingleton<ConfigurationPageViewModel>();
                services.AddSingleton<OpenAIConfigurationPage>();
                services.AddSingleton<OpenAIConfigurationPageViewModel>();
                services.AddSingleton<StoragePage>();
                services.AddSingleton<StoragePageViewModel>();
                services.AddSingleton<ThreadsPage>();
                services.AddSingleton<ThreadsPageViewModel>();
                services.AddSingleton<ConversationWindow>();
                services.AddSingleton<ConversationWindowViewModel>();
                services.AddSingleton<AboutPage>();
                services.AddSingleton<AboutPageViewModel>();
            }).Build();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            bool isNewInstance;
            mutex = new Mutex(true, "DesktopAssistantAIMutex", out isNewInstance);

            if (!isNewInstance)
            {
                MessageBoxHelper.ShowMessageAsync("Information", "The application is already running.");
                Environment.Exit(0);
            }

            string exeConfigurationPath = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            Debug.Print("Configuration Path: " + exeConfigurationPath);

            if (DesktopAssistantAI.Properties.Settings.Default.UpgradeRequired)
            {
                DesktopAssistantAI.Properties.Settings.Default.Upgrade();
                DesktopAssistantAI.Properties.Settings.Default.UpgradeRequired = false;
                DesktopAssistantAI.Properties.Settings.Default.Save();
            }

            var savedConfig = DesktopAssistantAI.Properties.Settings.Default.AssistantsApiConfigs;
            if (!string.IsNullOrEmpty(savedConfig))
            {
                AssistantsApiConfigs = JsonHelper.DeserializeAssistantsApiConfigCollection(savedConfig);
            }
            else
            {
                AssistantsApiConfigs = new AssistantsApiConfigCollection();
            }

            var savedApiConfig = DesktopAssistantAI.Properties.Settings.Default.OpenAIApiConfigs;
            if (!string.IsNullOrEmpty(savedApiConfig))
            {
                OpenAIApiConfigs = JsonHelper.DeserializeOpenAIApiConfigCollection(savedApiConfig);
            }
            else
            {
                OpenAIApiConfigs = new OpenAIApiConfigCollection();
            }

            var savedAvatarConfig = DesktopAssistantAI.Properties.Settings.Default.AvatarConfigs;
            if (!(string.IsNullOrEmpty(savedAvatarConfig) || savedAvatarConfig == "[]"))
            {
                AvatarConfigs = JsonHelper.DeserializeAvatarConfigCollection(savedAvatarConfig);
                AvatarConfigs.CollectionChanged += AvatarConfigs_CollectionChanged;
            }
            else
            {
                AvatarConfigs = new AvatarConfigCollection();
                AvatarConfigs.CollectionChanged += AvatarConfigs_CollectionChanged;

                var initialItems = new List<AvatarConfig>
                {
                    new AvatarConfig { AvatarName = "OpenAI", CreationType = "BuiltIn", DisplayType = "Image", AvatarImagePath = "pack://application:,,,/Assets/OpenAI.png" },
                };

                foreach (var item in initialItems)
                {
                    AvatarConfigs.Add(item);
                }
            }

            _host.Start();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            mutex.ReleaseMutex();

            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private AssistantsApiConfigCollection _assistantsApiConfigs;

        public AssistantsApiConfigCollection AssistantsApiConfigs
        {
            get => _assistantsApiConfigs;
            set
            {
                _assistantsApiConfigs = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("AssistantsApiConfigs"));
                }

                if (_assistantsApiConfigs != null)
                {
                    var settings = DesktopAssistantAI.Properties.Settings.Default;
                    settings.AssistantsApiConfigs = JsonHelper.SerializeAssistantsApiConfigCollection(_assistantsApiConfigs);
                    settings.Save();
                }
            }
        }

        private OpenAIApiConfigCollection _openAIApiConfigs;

        public OpenAIApiConfigCollection OpenAIApiConfigs
        {
            get => _openAIApiConfigs;
            set
            {
                _openAIApiConfigs = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("OpenAIApiConfigs"));
                }

                if (_openAIApiConfigs != null)
                {
                    var settings = DesktopAssistantAI.Properties.Settings.Default;
                    settings.OpenAIApiConfigs = JsonHelper.SerializeOpenAIApiConfigCollection(_openAIApiConfigs);
                    settings.Save();
                }
            }
        }

        private AvatarConfigCollection _avatarConfigs;

        public AvatarConfigCollection AvatarConfigs
        {
            get => _avatarConfigs;
            set
            {
                _avatarConfigs = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("AvatarConfigs"));
                }
            }
        }

        private void AvatarConfigs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var settings = DesktopAssistantAI.Properties.Settings.Default;
            settings.AvatarConfigs = JsonHelper.SerializeAvatarConfigCollection(AvatarConfigs);
            settings.Save();

            OnPropertyChanged("AvatarConfigs");
        }

    }
}
