using System.Collections;
using System.Globalization;
using System.Windows.Data;
namespace DesktopAssistantAI.Helpers.Converters;

public class EmptyOrNullArrayToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var collection = value as ICollection;
        return collection == null || collection.Count == 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
