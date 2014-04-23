using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RTWin.Converters
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
            return ToSince((DateTime.Now - date), "ago");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return DateTime.ParseExact((string)value, "d", culture);
            throw new NotImplementedException();
        }

        private static string ToSince(TimeSpan timespan, string append = "")
        {
            string message = "";
            if (timespan.Days > 365)
            {
                var years = Math.Floor(timespan.TotalDays / 365);
                message = years == 1 ? "1 year" : years + " years";
            }
            else if (timespan.Days > 30)
            {
                var months = Math.Floor(timespan.TotalDays / 30);
                message = months == 1 ? "1 month" : months + " months";
            }
            else if (timespan.Days == 1)
            {
                message = "1 day";
            }
            else if (timespan.Days > 0)
            {
                message = Math.Floor(timespan.TotalDays) + " days";
            }
            else if (timespan.Hours == 1)
            {
                message = "1 hour";
            }
            else if (timespan.Hours > 0)
            {
                message = Math.Floor(timespan.TotalHours) + " hours";
            }
            else if (timespan.Minutes > 5)
            {
                message = Math.Floor(timespan.TotalMinutes) + " minutes";
            }
            else if (timespan.Minutes > 1)
            {
                message = "minutes";
            }
            else
            {
                message = "seconds";
            }

            return string.IsNullOrEmpty(append) ? message : message + " " + append;
        }
    }
}
