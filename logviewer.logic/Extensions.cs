// Created by: egr
// Created at: 20.09.2012
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Humanizer;
using logviewer.engine;
using logviewer.logic.Properties;
using logviewer.logic.support;

namespace logviewer.logic
{
    public static class Extensions
    {
        internal static string ToParameterName(this LogLevel level)
        {
            return level.ToString(@"G") + @"Color";
        }

        internal static  bool HasProperty(this ICollection<Semantic> schema, ParserType type)
        {
            return schema.SelectMany(s => s.CastingRules).Any(r => r.Type == type);
        }

        internal static string PropertyNameOf(this ICollection<Semantic> schema, ParserType type)
        {
            return (from s in schema from rule in s.CastingRules where rule.Type == type select s.Property).FirstOrDefault();
        }

        /// <summary>
        /// Whether the string is valid to use as text filter
        /// </summary>
        /// <param name="messageTextFilter">string that supposed to be a filter</param>
        /// <param name="useRegularExpressions">Wheter to use regular expressions in filter string</param>
        /// <returns></returns>
        public static bool IsValid(this string messageTextFilter, bool useRegularExpressions)
        {
            if (string.IsNullOrEmpty(messageTextFilter) || !useRegularExpressions)
            {
                return true;
            }
            try
            {
                var r = new Regex(messageTextFilter, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                return r.GetHashCode() > 0;
            }
            catch (Exception e)
            {
                Log.Instance.Info(e.Message, e);
                return false;
            }
        }

        public static int ToSafePercent(this int value, int min, int max)
        {
            if (value > max)
            {
                return max;
            }
            return value < min ? min : value;
        }

        private static string BytesToString(this ulong bytes)
        {
            return ((long)bytes).Declension(Resources.BytesNominative, Resources.BytesGenitiveSingular, Resources.BytesGenitivePlural);
        }

        public static string Format(this LoadProgress progress)
        {
            return progress.Speed.Bytes == 0
                ? string.Format(Resources.SpeedPercent, progress.Percent)
                : string.Format(Resources.SpeedPercentWithRemain, progress.Percent, progress.Speed.Format(), progress.Remainig.Humanize());
        
        }

        private const string BigFileFormatNoBytes = @"{0:F2} {1}";
        private const string SmallFileFormat = @"{0} {1}";

        public static string Format(this FileSize fileSize)
        {
            string[] sizes =
            {
                Resources.SizeBytes,
                Resources.SizeKBytes,
                Resources.SizeMBytes,
                Resources.SizeGBytes,
                Resources.SizeTBytes,
                Resources.SizePBytes,
                Resources.SizeEBytes
            };
            if (fileSize.Unit == SizeUnit.Bytes)
            {
                return string.Format(CultureInfo.CurrentCulture, SmallFileFormat, fileSize.Bytes,
                    fileSize.Bytes.BytesToString());
            }
            var noBytes = string.Format(CultureInfo.CurrentCulture,
                    BigFileFormatNoBytes,
                    fileSize.Value,
                    sizes[(int)fileSize.Unit]);
            if (fileSize.BigWithoutBytes)
            {
                return noBytes;
            }
            return noBytes + $" ({fileSize.Bytes.ToString(@"N0", CultureInfo.CurrentCulture)} {fileSize.Bytes.BytesToString()})";
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