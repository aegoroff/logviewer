// Created by: egr
// Created at: 20.09.2012
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using logviewer.core.Properties;
using logviewer.engine;

namespace logviewer.core
{
    public static class Extensions
    {
        internal static string ToParameterName(this LogLevel level)
        {
            return level.ToString("G") + "Color";
        }

        internal static  bool HasProperty(this ICollection<Semantic> schema, string type)
        {
            return schema.SelectMany(s => s.CastingRules).Any(r => r.Type.Contains(type));
        }

        internal static string PropertyNameOf(this ICollection<Semantic> schema, string type)
        {
            return (from s in schema from rule in s.CastingRules where rule.Type.Contains(type) select s.Property).FirstOrDefault();
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
            return input.Days > 0 ? string.Format(Resources.RemainingWithDays, input.Days, input.Hours, input.Minutes, input.Seconds, input.DaysToString(), input.HoursToString(), input.MinutesToString(), input.SecondsToString()) : null;
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
            return ((long)input.Seconds).Declension(Resources.SecondsNominative, Resources.SecondsGenitiveSingular, Resources.SecondsGenitivePlural);
        }
        
        private static string MinutesToString(this TimeSpan input)
        {
            return ((long)input.Minutes).Declension(Resources.MinutesNominative, Resources.MinutesGenitiveSingular, Resources.MinutesGenitivePlural);
        }
        
        private static string HoursToString(this TimeSpan input)
        {
            return ((long)input.Hours).Declension(Resources.HoursNominative, Resources.HoursGenitiveSingular, Resources.HoursGenitivePlural);
        }
        
        private static string DaysToString(this TimeSpan input)
        {
            return ((long)input.Days).Declension(Resources.DaysNominative, Resources.DaysGenitiveSingular, Resources.DaysGenitivePlural);
        }

        private static string BytesToString(this ulong bytes)
        {
            return ((long)bytes).Declension(Resources.BytesNominative, Resources.BytesGenitiveSingular, Resources.BytesGenitivePlural);
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

        public static string Format(this LoadProgress progress)
        {
            return progress.Speed.Bytes == 0
                ? string.Format(Resources.SpeedPercent, progress.Percent)
                : string.Format(Resources.SpeedPercentWithRemain, progress.Percent, progress.Speed.Format(), progress.Remainig.TimespanToHumanString());
        
        }

        private const string BigFileFormat = "{0:F2} {1} ({2} {3})";
        private const string BigFileFormatNoBytes = "{0:F2} {1}";
        private const string SmallFileFormat = "{0} {1}";

        private static readonly string[] sizes =
        {
            Resources.SizeBytes,
            Resources.SizeKBytes,
            Resources.SizeMBytes,
            Resources.SizeGBytes,
            Resources.SizeTBytes,
            Resources.SizePBytes,
            Resources.SizeEBytes
        };

        public static string Format(this FileSize fileSize)
        {
            if (fileSize.Unit == SizeUnit.Bytes)
            {
                return string.Format(CultureInfo.CurrentCulture, SmallFileFormat, fileSize.Bytes,
                    fileSize.Bytes.BytesToString());
            }
            if (fileSize.BigWithoutBytes)
            {
                return string.Format(CultureInfo.CurrentCulture, BigFileFormatNoBytes, fileSize.Value, sizes[(int)fileSize.Unit]);
            }
            return string.Format(CultureInfo.CurrentCulture, BigFileFormat, fileSize.Value,
                sizes[(int)fileSize.Unit], fileSize.Bytes.ToString(fileSize.Bytes.FormatString(), CultureInfo.CurrentCulture),
                fileSize.Bytes.BytesToString());
        }

        private static readonly IDictionary<int, Func<long, string, string, string, string>> declensions = new Dictionary
            <int, Func<long, string, string, string, string>>
        {
            { 1049, DeclensionRu }
        };

        private static string Declension(this long number, string nominative, string genitiveSingular,
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
         private static string DeclensionRu(long number, string nominative, string genitiveSingular, string genitivePlural)
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
         private static string DeclensionEn(long number, string nominative, string genitiveSingular, string genitivePlural)
         {
             if (number == 1 || number == -1)
             {
                 return nominative;
             }
             return genitiveSingular ?? genitivePlural;
         }
    }
}