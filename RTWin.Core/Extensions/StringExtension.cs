using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RTWin.Core.Extensions
{
    public static class StringExtension
    {
        public static bool IsValidRegex(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            try
            {
                Regex.Match("", input);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
