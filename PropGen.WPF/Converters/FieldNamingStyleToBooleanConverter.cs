using System.Globalization;
using System.Windows.Data;
using PropGen.Core.Models;

namespace PropGen.WPF.Converters
{
   
    public class FieldNamingStyleToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (value is KeyValuePair<FieldNamingStyle, string> style)
            {
                return style.Key == FieldNamingStyle.PrefixedCamelCase;
            }

            return false;
        }
                
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("FieldNamingStyleToBooleanConverter only supports one-way binding.");
        }
    }
}
