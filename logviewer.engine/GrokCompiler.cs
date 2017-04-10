// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 06.07.2015
// © 2012-2017 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace logviewer.engine
{
    /// <summary>
    ///     Represents grok template compiler
    /// </summary>
    internal class GrokCompiler
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private Action<string> customErrorOutputMethod;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private List<Semantic> messageSchema = new List<Semantic>();

        private const string MainPattern = "MAIN"; // Not L10N

        private string[] patternFiles;

        private const int StreamBufferSize = 0xFFFF;

        /// <summary>
        ///     Creates new compiler instance using custom error method output if necessary
        /// </summary>
        /// <param name="customErrorOutputMethod"></param>
        internal GrokCompiler(Action<string> customErrorOutputMethod = null)
        {
            this.customErrorOutputMethod = customErrorOutputMethod;
            this.CreateLibraryTemplates();
        }

        private void CreateLibraryTemplates()
        {
            const string pattern = "*.patterns"; // Not L10N
            this.patternFiles = Directory.GetFiles(Extensions.AssemblyDirectory, pattern, SearchOption.TopDirectoryOnly);
            if (this.patternFiles.Length == 0)
            {
                this.patternFiles = Directory.GetFiles(".", pattern, SearchOption.TopDirectoryOnly); // Not L10N
            }
        }

        /// <summary>
        ///     Compiles grok specified
        /// </summary>
        /// <param name="grok"></param>
        /// <returns>Regular expression</returns>
        internal string Compile(string grok)
        {
            this.messageSchema.Clear();
            var parser = new grammar.GrokParser(this.customErrorOutputMethod);

            foreach (var stream in this.patternFiles.Select(File.OpenRead))
            {
                var bufferedStream = new BufferedStream(stream, StreamBufferSize);
                using (bufferedStream)
                {
                    parser.Parse(stream);
                }
            }

            var mainDefinition = $"{MainPattern} {grok}";
            parser.Parse(mainDefinition);

            var mainTranslated = parser.DefinitionsTable[MainPattern];
            return mainTranslated.Compose(this.messageSchema);
        }

        /// <summary>
        /// Message schema - all possible properties and casting rules
        /// </summary>
        internal IEnumerable<Semantic> MessageSchema => this.messageSchema;
    }
}
