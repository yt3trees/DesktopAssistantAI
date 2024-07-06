using System.Collections.ObjectModel;

namespace DesktopAssistantAI.Helpers;

public class AvatarControlHelper
{
    public static int SetAvatarSize(string sizeType)
    {
        if (sizeType == "Large")
        {
            return 200;
        }
        if (sizeType == "Medium")
        {
            return 150;
        }
        if (sizeType == "Small")
        {
            return 100;
        }
        if (sizeType == "ExtraSmall")
        {
            return 50;
        }
        else
        {
            return 150;
        }
    }

    public static ObservableCollection<string> GetAvatarSizeOptions()
    {
        return new ObservableCollection<string>
        {
            "Large",
            "Medium",
            "Small",
            "ExtraSmall",
        };
    }
}
