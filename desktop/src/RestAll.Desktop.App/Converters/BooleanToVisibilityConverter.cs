using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RestAll.Desktop.App.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = value is bool b && b;
        var inverse = parameter is string s && s.Equals("inverse", StringComparison.OrdinalIgnoreCase);
        
        if (inverse)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility visibility && visibility == Visibility.Visible;
    }
}
