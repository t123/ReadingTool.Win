using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Humanizer;

namespace RTWin.Common
{
    [ValueConversion(typeof(DateTime), typeof(String))]
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "never";
            }

            DateTime date = (DateTime)value;

            //return date.ToString("d");
            return date.Humanize();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return DateTime.ParseExact((string)value, "d", culture);
            throw new NotImplementedException();
        }
    }
}
