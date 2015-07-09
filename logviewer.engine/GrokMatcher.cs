// Created by: egr
// Created at: 02.10.2014
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace logviewer.engine
{
    /// <summary>
    /// Represents pattern mather
    /// </summary>
    public class GrokMatcher
    {
        private Regex regex;
        private readonly List<Semantic> messageSchema = new List<Semantic>();
        private readonly Action<string> customErrorOutputMethod;

        /// <summary>
        /// Init new matcher
        /// </summary>
        /// <param name="grok">Template definition</param>
        /// <param name="options">Result regexp options</param>
        /// <param name="customErrorOutputMethod"></param>
        public GrokMatcher(string grok, RegexOptions options = RegexOptions.None, Action<string> customErrorOutputMethod = null)
        {
            this.customErrorOutputMethod = customErrorOutputMethod;
            this.CreateRegexp(grok, options);
        }

        private void CreateRegexp(string grok, RegexOptions options)
        {
            try
            {
                this.Template = this.Compile(grok);
                this.regex = new Regex(this.Template, options);
            }
            catch (Exception e)
            {
                if (this.customErrorOutputMethod != null)
                {
                    this.customErrorOutputMethod(e.ToString());
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine(e.ToString());
                }
                this.CompilationFailed = true;
                this.Template = grok;
            }
        }

        private string Compile(string grok)
        {
            var compiler = new GrokCompiler();
            var result = compiler.Compile(grok);
            this.messageSchema.AddRange(compiler.MessageSchema);
            return result;
        }

        /// <summary>
        /// Regexp that compiled from pattern (grok definition)
        /// </summary>
        public string Template { get; private set; }

        /// <summary>
        /// Whether the grok compilation failed or not
        /// </summary>
        public bool CompilationFailed { get; private set; }

        /// <summary>
        /// Message schema - all possible properties and casting rules
        /// </summary>
        public ICollection<Semantic> MessageSchema
        {
            get { return this.messageSchema; }
        }

        /// <summary>
        /// Checks line matching the pattern
        /// </summary>
        /// <param name="s">string to validate</param>
        /// <returns>True if string matches the pattern false otherwise </returns>
        public bool Match(string s)
        {
            return this.regex != null && this.regex.IsMatch(s);
        }

        /// <summary>
        /// Parse line and extract metadata that message schema defines
        /// </summary>
        /// <param name="s">String to parse</param>
        /// <returns>Metadata dictionary or null</returns>
        public IDictionary<string, string> Parse(string s)
        {
            if (this.regex == null)
            {
                return null;
            }
            var match = this.regex.Match(s);
            return !match.Success ? null : this.MessageSchema.ToDictionary(semantic => semantic.Property, semantic => match.Groups[semantic.Property].Value);
        }
    }
}