using DesktopAssistantAI.ViewModels.Windows;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace DesktopAssistantAI.Views.Windows;

public partial class MainWindow : INavigationWindow
{
    private bool isDragging = false;

    private Point clickPosition;

    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel viewModel,
        IPageService pageService,
        INavigationService navigationService,
        IServiceProvider serviceProvider,
        IContentDialogService contentDialogService
    )
    {
        ViewModel = viewModel;

        InitializeComponent();

        //SystemThemeWatcher.Watch(this);

        RecoverWindowBounds();

        //SetPageService(pageService);
        //navigationService.SetNavigationControl(RootNavigation);
    }

    #region INavigationWindow methods

    public INavigationView GetNavigation() => null;

    //public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);
    public bool Navigate(Type pageType) => false;

    public void SetPageService(IPageService pageService) { }

    public void ShowWindow() => Show();

    public void CloseWindow() => Close();

    #endregion INavigationWindow methods


    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            isDragging = true;
            clickPosition = e.GetPosition(this);
            Mouse.Capture(this);
        }
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (isDragging)
        {
            Point currentPosition = e.GetPosition(this);
            double offsetX = currentPosition.X - clickPosition.X;
            double offsetY = currentPosition.Y - clickPosition.Y;

            double newLeft = this.Left + offsetX;
            double newTop = this.Top + offsetY;

            this.Left = newLeft;
            this.Top = newTop;
        }
    }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (isDragging)
        {
            isDragging = false;
            Mouse.Capture(null);
        }
    }

    void RecoverWindowBounds()
    {
        var settings = DesktopAssistantAI.Properties.Settings.Default;
        if (settings.WindowLeft >= 0 &&
            settings.WindowLeft < SystemParameters.VirtualScreenWidth)
        { Left = settings.WindowLeft; }
        if (settings.WindowTop >= 0 &&
            settings.WindowTop < SystemParameters.VirtualScreenHeight)
        { Top = settings.WindowTop; }
        if (settings.WindowMaximized)
        {
            Loaded += (o, e) => WindowState = WindowState.Maximized;
        }
    }

    private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    /// <summary>
    /// Raises the closed event.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // Make sure that closing this window will begin the process of closing the application.
        Application.Current.Shutdown();
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }

    private void HorizontalThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        double newWidth = ResizableBorder.ActualWidth - e.HorizontalChange;
        double maxWidth = MainGrid.ColumnDefinitions[0].ActualWidth;
        double minWidth = 200;

        if (newWidth > minWidth && newWidth <= maxWidth)
        {
            ResizableBorder.Width = newWidth;
        }
    }
    private void VerticalThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        double newHeight = ResizableBorder.ActualHeight - e.VerticalChange;
        double maxHeight = MainGrid.RowDefinitions[0].ActualHeight;
        double minHeight = 50;

        if (newHeight > minHeight && newHeight <= maxHeight)
        {
            ResizableBorder.Height = newHeight;
        }
    }

    private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        double newWidth = ResizableBorder.ActualWidth - e.HorizontalChange;
        double maxWidth = MainGrid.ColumnDefinitions[0].ActualWidth;
        double minWidth = 200;

        if (newWidth > minWidth && newWidth <= maxWidth)
        {
            ResizableBorder.Width = newWidth;
        }

        double newHeight = ResizableBorder.ActualHeight - e.VerticalChange;
        double maxHeight = MainGrid.RowDefinitions[0].ActualHeight;
        double minHeight = 50;

        if (newHeight > minHeight && newHeight <= maxHeight)
        {
            ResizableBorder.Height = newHeight;
        }
    }

    private void ResetPosition_Click(object sender, RoutedEventArgs e)
    {
        this.Left = -100;
        this.Top = -200;
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
