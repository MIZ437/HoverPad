using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HoverPad.Converters;

public class GapToMarginConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int gap)
        {
            var margin = gap / 2.0;
            return new Thickness(margin);
        }
        return new Thickness(4);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
