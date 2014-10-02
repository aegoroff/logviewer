// Created by: egr
// Created at: 02.10.2014
// © 2012-2014 Alexander Egorov

using System;
using System.IO;
using Antlr4.Runtime;

namespace logviewer.core
{
    public class GrokMatcher
    {
        public bool Match(string s)
        {
            try
            {
                ICharStream stream = new UnbufferedCharStream(new StringReader(s));
                GrokLexer gl = new GrokLexer(stream);
                CommonTokenStream ts = new CommonTokenStream(gl);
                GrokParser gp = new GrokParser(ts);
                gp.parse();
            }
            catch (NotSupportedException e)
            {
                Log.Instance.Trace(e);
                return false;
            }
            return true;
        }
    }
}