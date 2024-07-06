using OpenAI.ObjectModels.ResponseModels.VectorStoreResponseModels;
using DesktopAssistantAI.Models;
using DesktopAssistantAI.ViewModels.SubWindows;

namespace DesktopAssistantAI.Views.SubWindows;

/// <summary>
/// VectorStoreInfo.xaml の相互作用ロジック
/// </summary>
public partial class VectorStoreInfo
{
    public VectorStoreInfo(VectorStoreObjectResponse vectorStoreObject, FileObjects fileObjects, string configurationName)
    {
        InitializeComponent();
        DataContext = new VectorStoreInfoViewModel(vectorStoreObject, fileObjects, configurationName);
    }
}
