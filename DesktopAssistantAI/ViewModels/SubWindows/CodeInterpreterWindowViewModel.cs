using ICSharpCode.AvalonEdit.Document;
using System.IO;
using System.Windows.Media.Imaging;

namespace DesktopAssistantAI.ViewModels.SubWindows;

public partial class CodeInterpreterWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _input = string.Empty;

    [ObservableProperty]
    private TextDocument _document;

    [ObservableProperty]
    private string _output = string.Empty;

    [ObservableProperty]
    private BitmapImage _imageFile;

    public CodeInterpreterWindowViewModel(string input, string output, byte[] imageFile)
    {
        Input = input;
        Output = output;

        if (imageFile.Length != 0)
        {
            ImageFile = new BitmapImage();
            ImageFile.BeginInit();
            ImageFile.StreamSource = new MemoryStream(imageFile);
            ImageFile.CacheOption = BitmapCacheOption.OnLoad;
            ImageFile.EndInit();
        }
    }

    public CodeInterpreterWindowViewModel()
    {
    }
}
