using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SBrickey.Libraries.WPF.Converters
{
    public class BoolValuesEqual : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // use System.Convert to cast the parameter (incoming as string) to the same type as [value]
            var typedParameter = System.Convert.ChangeType(parameter, value.GetType());

            // then use simple Equals to compare
            return value.Equals(typedParameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    } // class
} // namespace