using System.Globalization;
using System.Windows.Data;

namespace DesktopAssistantAI.Helpers.Converters;

public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var parameterString = parameter as string;
        if (null == parameterString)
            return DependencyProperty.UnsetValue;

        if (!Enum.IsDefined(value.GetType(), value))
            return DependencyProperty.UnsetValue;

        var parameterValue = Enum.Parse(value.GetType(), parameterString);
        return parameterValue.Equals(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var parameterString = parameter as string;
        if (null == parameterString)
            return DependencyProperty.UnsetValue;

        if (true.Equals(value))
            return Enum.Parse(targetType, parameterString);
        else
            return DependencyProperty.UnsetValue;
    }
}
