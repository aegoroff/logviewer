using System;
using System.Collections.Generic;
using System.IO;

namespace logviewer.engine
{
    /// <summary>
    ///     Represents grok template compiler
    /// </summary>
    internal class GrokCompiler
    {
        private readonly Action<string> customErrorOutputMethod;
        private readonly Dictionary<string, string> templates = new Dictionary<string, string>();
        private readonly List<Semantic> messageSchema = new List<Semantic>();

        /// <summary>
        ///     Creates new compiler instance using custom error method output if necessary
        /// </summary>
        /// <param name="customErrorOutputMethod"></param>
        internal GrokCompiler(Action<string> customErrorOutputMethod = null)
        {
            this.customErrorOutputMethod = customErrorOutputMethod;
            this.CreateTemplates();
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

        /// <summary>
        ///     Compiles grok specified
        /// </summary>
        /// <param name="grok"></param>
        /// <returns>Regular expression</returns>
        internal string Compile(string grok)
        {
            var parser = new grammar.GrokParser(this.templates, this.Compile, this.customErrorOutputMethod); // recursion here
            parser.Parse(grok);
            this.messageSchema.AddRange(parser.Schema);
            var result = parser.Template;
            if (!parser.IsPropertyStackEmpty)
            {
                throw new Exception("Unused property detected");
            }
            return result;
        }

        /// <summary>
        /// Message schema - all possible properties and casting rules
        /// </summary>
        internal ICollection<Semantic> MessageSchema
        {
            get { return this.messageSchema; }
        }
    }
}