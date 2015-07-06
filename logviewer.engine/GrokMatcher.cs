// Created by: egr
// Created at: 02.10.2014
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;

namespace logviewer.engine
{
    /// <summary>
    /// Represents pattern mather
    /// </summary>
    public class GrokMatcher
    {
        private Regex regex;
        private readonly List<Semantic> messageSchema = new List<Semantic>();
        private readonly Dictionary<string, string> templates = new Dictionary<string, string>();

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
            this.CreateTemplates();
            this.CreateRegexp(grok, options);
        }

        private void CreateTemplates()
        {
            const string pattern = "*.patterns";
            var patternFiles = Directory.GetFiles(Extensions.AssemblyDirectory, pattern, SearchOption.TopDirectoryOnly);
            if (patternFiles.Length == 0)
            {
                patternFiles = Directory.GetFiles(".", pattern, SearchOption.TopDirectoryOnly);
            }
            foreach (var file in patternFiles)
            {
                this.AddTemplates(file);
            }
        }

        private void AddTemplates(string fullPath)
        {
            var patterns = File.ReadAllLines(fullPath);
            foreach (var pattern in patterns)
            {
                var parts = pattern.Split(new[] { ' ' }, StringSplitOptions.None);
                if (parts.Length < 2)
                {
                    continue;
                }
                var template = parts[0];
                if (string.IsNullOrWhiteSpace(template) || template.StartsWith("#") || this.templates.ContainsKey(template))
                {
                    continue;
                }
                this.templates.Add(template, pattern.Substring(template.Length).Trim());
            }
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
            ICharStream inputStream = new AntlrInputStream(grok);
            var lexer = new GrokLLLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new GrokLLParser(tokenStream);
            var tree = parser.parse();
#if DEBUG
            parser.Trace = true;
            parser.RemoveErrorListeners();
            parser.Interpreter.PredictionMode = PredictionMode.LlExactAmbigDetection;
            parser.AddErrorListener(new DiagnosticErrorListener());
            if (this.customErrorOutputMethod != null)
            {
                parser.AddErrorListener(new CustomErrorListener(this.customErrorOutputMethod));
            }
#endif

            this.CompilationFailed = parser.NumberOfSyntaxErrors > 0;

            if (this.CompilationFailed)
            {
                throw new ArgumentException("Invalid pattern: " + grok, "grok");
            }

            var grokVisitor = new GrokVisitor(this.templates, this.Compile);
            grokVisitor.Visit(tree);
            var result = grokVisitor.Template; // All nested templates compiled here
            this.messageSchema.AddRange(grokVisitor.Schema);
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

    internal class CustomErrorListener : IAntlrErrorListener<IToken>
    {
        private readonly Action<string> outputMethod;

        internal CustomErrorListener(Action<string> outputMethod)
        {
            this.outputMethod = outputMethod;
        }

        public static readonly ConsoleErrorListener<IToken> Instance = new ConsoleErrorListener<IToken>();

        public virtual void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            this.outputMethod("line " + line + ":" + charPositionInLine + " " + msg);
        }
    }
}