using System;

namespace logviewer.rtf
{
    public enum MetricUnit { Point, Millimeter, Centimeter, Inch }
    
    /// <summary>
    /// Converts twips to metric units, and metric units to twips
    /// </summary>
    public class TwipConverter
    {
        public const float 
            PointsInTwip = .05F,
            MillimetersInTwip = .0176389F,
            CentimetersInTwip = .0017639F,
            InchsInTwip = 1F / 1440,
 
            TwipsInPoint = 20,
            TwipsInMillimeter = 56.6929134F,
            TwipsInCentimeter = 566.9291339F,
            TwipsInInch = 1440F;

        private static readonly float[]
            toUnitConversion =
            {
                PointsInTwip,
                MillimetersInTwip,
                CentimetersInTwip,
                InchsInTwip,
            };

        private static readonly float[]
            toTwipConversion =
            {
                TwipsInPoint,
                TwipsInMillimeter,
                TwipsInCentimeter,
                TwipsInInch,
            };

        /// <summary>
        /// Converts metric values to twips
        /// </summary>
        /// <param name="value">Value in metric units</param>
        /// <param name="unit">Unit which is used to specify the value</param>
        /// <returns>Value in twips</returns>
        public static int ToTwip(float value, MetricUnit unit)
        {   
            return (int)Math.Round(value * toTwipConversion[(int)unit]);
        }

        /// <summary>
        /// Converts twips to points
        /// </summary>
        /// <param name="value">Value in twips</param>
        /// <returns>Value in points</returns>
        public static float ToPoint(int value)
        {
            return ToMetricUnit(value, MetricUnit.Point);
        }

        /// <summary>
        /// Converts twips to millimeters
        /// </summary>
        /// <param name="value">Value in twips</param>
        /// <returns>Value in millimeters</returns>
        public static float ToMillimeter(int value)
        {
            return ToMetricUnit(value, MetricUnit.Millimeter);
        }

        /// <summary>
        /// Converts twips to centimeters
        /// </summary>
        /// <param name="value">Value in twips</param>
        /// <returns>Value in centimeters</returns>
        public static float ToCentimeter(int value)
        {
            return ToMetricUnit(value, MetricUnit.Centimeter);
        }

        /// <summary>
        /// Converts twips to metric values
        /// </summary>
        /// <param name="value">Value in twips</param>
        /// <param name="unit">Metric unit to convert to</param>
        /// <returns>Value in specified metric units</returns>
        public static float ToMetricUnit(int value, MetricUnit unit)
        {
            return (float)Math.Round(value * toUnitConversion[(int)unit]);
        }
    }
}