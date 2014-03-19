// Created by: egr
// Created at: 20.09.2012
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

        public static string FormatString(this ulong value)
        {
            if (value == 0)
            {
                return string.Empty;
            }
            Func<ulong, int> count = null;
            count = num => (num /= 10) > 0 ? 1 + count(num) : 1;

            var digits = count(value);

            var builder = new StringBuilder();
            for (var i = digits; i > 0; i--)
            {
                if (i % 3 == 0 && i < digits)
                {
                    builder.Append(' ');
                }
                builder.Append('#');
            }
            return builder.ToString();
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
            return input.Hours > 0 ? string.Format(Resources.RemainingWithHours, input.Hours, input.Minutes, input.Seconds, input.HoursToString(), input.MinutesToString(), input.SecondsToString()) : null;
        }

        private static string WithMinutes(TimeSpan input)
        {
            return input.Minutes > 0 ? string.Format(Resources.RemainingWithMinutes, input.Minutes, input.Seconds, input.MinutesToString(), input.SecondsToString()) : null;
        }

        private static string WithSeconds(TimeSpan input)
        {
            return input.Seconds > 0 ? string.Format(Resources.RemainingOnlySeconds, input.Seconds, input.SecondsToString()) : null;
        }

        private static string SecondsToString(this TimeSpan input)
        {
            return input.Seconds.Declension(Resources.SecondsNominative, Resources.SecondsGenitiveSingular, Resources.SecondsGenitivePlural);
        }
        
        private static string MinutesToString(this TimeSpan input)
        {
            return input.Minutes.Declension(Resources.MinutesNominative, Resources.MinutesGenitiveSingular, Resources.MinutesGenitivePlural);
        }
        
        private static string HoursToString(this TimeSpan input)
        {
            return input.Hours.Declension(Resources.HoursNominative, Resources.HoursGenitiveSingular, Resources.HoursGenitivePlural);
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

        private static readonly IDictionary<int, Func<int, string, string, string, string>> declensions = new Dictionary
            <int, Func<int, string, string, string, string>>
        {
            { 1049, DeclensionRu }
        };

        private static string Declension(this int number, string nominative, string genitiveSingular,
            string genitivePlural)
        {
            return declensions.ContainsKey(Thread.CurrentThread.CurrentUICulture.LCID)
                ? declensions[Thread.CurrentThread.CurrentUICulture.LCID](number, nominative, genitiveSingular, genitivePlural)
                : DeclensionEn(number, nominative, genitiveSingular, genitivePlural);
        }

        /// <summary>
         /// Does a word declension after a number.
         /// </summary>
         /// <param name="number"></param>
         /// <param name="nominative"></param>
         /// <param name="genitiveSingular"></param>
         /// <param name="genitivePlural"></param>
         /// <returns></returns>
         private static string DeclensionRu(int number, string nominative, string genitiveSingular, string genitivePlural)
         {
             var lastDigit = number % 10;
             var lastTwoDigits = number % 100;
             if (lastDigit == 1 && lastTwoDigits != 11)
             {
                 return nominative;
             }
             if (lastDigit == 2 && lastTwoDigits != 12 || lastDigit == 3 && lastTwoDigits != 13 || lastDigit == 4 && lastTwoDigits != 14)
             {
                 return genitiveSingular;
             }
             return genitivePlural;
         }
        
        /// <summary>
         /// Does a word declension after a number.
         /// </summary>
         /// <param name="number"></param>
         /// <param name="nominative"></param>
         /// <param name="genitiveSingular"></param>
         /// <param name="genitivePlural"></param>
         /// <returns></returns>
         private static string DeclensionEn(int number, string nominative, string genitiveSingular, string genitivePlural)
         {
             if (number == 1 || number == -1)
             {
                 return nominative;
             }
             return genitiveSingular ?? genitivePlural;
         }
    }
}