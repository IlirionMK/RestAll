using System.Globalization;
using System.Windows.Data;
using RestAll.Desktop.Core.Orders;

namespace RestAll.Desktop.App.Converters;

/// <summary>
/// Converter to detect offline orders (negative IDs) and return appropriate visibility/brush values
/// </summary>
public class OfflineOrderConverter : IValueConverter
{
    /// <summary>
    /// Converts order ID to Visibility - Visible if offline (negative ID), Collapsed otherwise
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Order order)
        {
            bool isOffline = order.Id < 0;
            
            // For Visibility binding
            if (targetType == typeof(System.Windows.Visibility))
            {
                return isOffline 
                    ? System.Windows.Visibility.Visible 
                    : System.Windows.Visibility.Collapsed;
            }
            
            // For Boolean binding
            if (targetType == typeof(bool))
            {
                return isOffline;
            }
            
            // For Brush binding (orange for offline)
            if (targetType == typeof(System.Windows.Media.Brush))
            {
                return isOffline 
                    ? System.Windows.Media.Brushes.Orange 
                    : System.Windows.Media.Brushes.Black;
            }
        }
        
        // Default fallback
        if (targetType == typeof(System.Windows.Visibility))
            return System.Windows.Visibility.Collapsed;
        if (targetType == typeof(bool))
            return false;
        if (targetType == typeof(System.Windows.Media.Brush))
            return System.Windows.Media.Brushes.Black;
            
        return System.Windows.DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
