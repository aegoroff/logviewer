// Created by: egr
// Created at: 02.10.2014
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;

namespace logviewer.engine
{
    /// <summary>
    /// Represents pattern mather
    /// </summary>
    public class GrokMatcher
    {
        private Regex regex;
        private readonly List<Semantic> messageSchema = new List<Semantic>();

        /// <summary>
        /// Init new matcher
        /// </summary>
        /// <param name="grok">Template definition</param>
        /// <param name="options">Result regexp options</param>
        public GrokMatcher(string grok, RegexOptions options = RegexOptions.None)
        {
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
                System.Diagnostics.Trace.WriteLine(e.ToString());
                this.CompilationFailed = true;
                this.Template = grok;
            }
        }

        private string Compile(string grok)
        {
            ICharStream inputStream = new AntlrInputStream(grok);
            var lexer = new GrokLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new GrokParser(tokenStream);
            var tree = parser.parse();

            this.CompilationFailed = parser.NumberOfSyntaxErrors > 0;

            if (this.CompilationFailed)
            {
                throw new ArgumentException("Invalid pattern: " + grok, "grok");
            }
            var grokVisitor = new GrokVisitor();
            grokVisitor.Visit(tree);

            this.messageSchema.AddRange(grokVisitor.Schema);
            if (!grokVisitor.RecompilationNeeded)
            {
                return grokVisitor.Template;
            }
            foreach (var ix in grokVisitor.RecompileIndexes)
            {
                var recompiled = this.Compile(grokVisitor.GetRecompile(ix));
                grokVisitor.SetRecompiled(ix, recompiled);
            }
            return grokVisitor.Template;
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