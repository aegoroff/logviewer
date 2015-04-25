// Created by: egr
// Created at: 02.10.2014
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using logviewer.engine.Tree;

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

        /// <summary>
        /// Init new matcher
        /// </summary>
        /// <param name="grok">Template definition</param>
        /// <param name="options">Result regexp options</param>
        public GrokMatcher(string grok, RegexOptions options = RegexOptions.None)
        {
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
#if DEBUG
            parser.Trace = true;
#endif

            this.CompilationFailed = parser.NumberOfSyntaxErrors > 0;

            if (this.CompilationFailed)
            {
                throw new ArgumentException("Invalid pattern: " + grok, "grok");
            }
            var root = new BinaryTreeNode<Pattern>(new StringLiteral(string.Empty));
            var grokVisitor = new GrokVisitor(this.templates, root, this.Compile);
            grokVisitor.Visit(tree);
            this.messageSchema.AddRange(grokVisitor.Schema);
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
            return this.regex.Return(r => r.IsMatch(s), false);
        }

        /// <summary>
        /// Parse line and extract metadata that message schema defines
        /// </summary>
        /// <param name="s">String to parse</param>
        /// <returns>Metadata dictionary or null</returns>
        public IDictionary<string, string> Parse(string s)
        {
            return this.regex
                .With(r => r.Match(s))
                .If(m => m.Success)
                .Return(m => this.MessageSchema.ToDictionary(semantic => semantic.Property, semantic => m.Groups[semantic.Property].Value), null);
        }
    }
}