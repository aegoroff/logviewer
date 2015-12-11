// Created by: egr
// Created at: 17.09.2013
// © 2012-2015 Alexander Egorov

using System;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace logviewer.logic
{
    [SQLiteFunction(Name = "REGEXP", Arguments = 2, FuncType = FunctionType.Scalar)]
    internal class SqliteRegEx : SQLiteFunction
    {
        private static string invalid;

        public override object Invoke(object[] args)
        {
            var input = Convert.ToString(args[1]);
            var pattern = Convert.ToString(args[0]);
            try
            {
                if (invalid != null && invalid.Equals(pattern))
                {
                    return false;
                }
                return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e.Message, e);
                invalid = pattern;
            }
            return false;
        }
    }
}