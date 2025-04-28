using System.Globalization;
using System.Windows.Data;

namespace PropGen.WPF.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return !boolean;
            }
            throw new InvalidOperationException("The target must be a boolean");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return !boolean;
            }
            throw new InvalidOperationException("The target must be a boolean");
        }
    }
}
