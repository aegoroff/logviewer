// Created by: egr
// Created at: 20.09.2012
// © 2012-2013 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using logviewer.core.Properties;

namespace logviewer.core
{
    public static class Extensions
    {
        internal static Regex ToMarker(this string marker)
        {
            return new Regex(marker, RegexOptions.Compiled);
        }

        /// <summary>
        ///     Decodes string into encoding specified
        /// </summary>
        /// <param name="line"> Source string </param>
        /// <param name="srcEncoding"> Source string encoding </param>
        /// <param name="dstEncoding"> Destination encoding </param>
        /// <returns>Decoded string</returns>
        internal static string Convert(this string line, Encoding srcEncoding, Encoding dstEncoding)
        {
            byte[] srcBytes = srcEncoding.GetBytes(line);
            byte[] dstBytes = Encoding.Convert(srcEncoding, dstEncoding, srcBytes);
            return dstEncoding.GetString(dstBytes);
        }

        private static int CountDigits(ulong num)
        {
            return (num /= 10) > 0 ? 1 + CountDigits(num) : 1;
        }

        public static string FormatString(this ulong value)
        {
            if (value == 0)
            {
                return string.Empty;
            }
            var digits = CountDigits(value);

            var list = new List<char>();
            for (var i = 0; i < digits; i++)
            {
                list.Add('#');
                if ((i + 1) % 3 == 0 && i + 1 < digits)
                {
                    list.Add(' ');
                }
            }
            list.Reverse();
            return new string(list.ToArray());
        }

        public static int ToSafePercent(this int value, int min, int max)
        {
            if (value > max)
            {
                return max;
            }
            return value < min ? min : value;
        }

        private static string WithDays(TimeSpan input)
        {
            return input.Days > 0 ? string.Format(Resources.RemainingWithDays, input.Days, input.Hours, input.Minutes, input.Seconds) : null;
        }

        private static string WithHours(TimeSpan input)
        {
            return input.Hours > 0 ? string.Format(Resources.RemainingWithHours, input.Hours, input.Minutes, input.Seconds) : null;
        }

        private static string WithMinutes(TimeSpan input)
        {
            return input.Minutes > 0 ? string.Format(Resources.RemainingWithMinutes, input.Minutes, input.Seconds) : null;
        }

        private static string WithSeconds(TimeSpan input)
        {
            return input.Seconds > 0 ? string.Format(Resources.RemainingOnlySeconds, input.Seconds) : null;
        }

        internal static string TimespanToHumanString(this TimeSpan timeSpan)
        {
            Func<TimeSpan, string>[] funcs =
            {
                WithDays,
                WithHours,
                WithMinutes,
                WithSeconds
            };
            foreach (var result in funcs.Select(format => format(timeSpan)).Where(result => result != null))
            {
                return result;
            }
            return Resources.RemainingLessThenSecond;
        }
    }
}