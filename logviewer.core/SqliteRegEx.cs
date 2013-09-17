using System;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace logviewer.core
{
    [SQLiteFunction(Name = "REGEXP", Arguments = 2, FuncType = FunctionType.Scalar)]
    internal class SqliteRegEx : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            var input = Convert.ToString(args[1]);
            var pattern = Convert.ToString(args[0]);
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
    }
}