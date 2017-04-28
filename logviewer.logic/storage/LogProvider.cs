// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 10.11.2015
// © 2012-2017 Alexander Egorov

using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using logviewer.engine;
using logviewer.logic.Annotations;
using logviewer.logic.models;
using logviewer.logic.ui;
using Net.Sgoliver.NRtfTree.Util;

namespace logviewer.logic.storage
{
    public sealed class LogProvider : IItemsProvider<string>, IDisposable
    {
        private readonly ISettingsProvider settings;

        private ILogStore store;

        private static readonly XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
                                                                      {
                                                                          OmitXmlDeclaration = true,
                                                                          ConformanceLevel = ConformanceLevel.Fragment,
                                                                          CheckCharacters = false,
                                                                          NewLineHandling = NewLineHandling.None
                                                                      };

        public LogProvider(ILogStore store, ISettingsProvider settings)
        {
            this.Store = store;
            this.settings = settings;
        }

        [PublicAPI]
        public MessageFilterViewModel FilterViewModel { get; set; }

        [PublicAPI]
        public ILogStore Store
        {
            get => this.store;
            set => this.store = value;
        }

        public void Dispose() => this.store?.Dispose();

        public long FetchCount()
        {
            return this.store?.CountMessages(this.FilterViewModel.Start, this.FilterViewModel.Finish, this.FilterViewModel.Min,
                                             this.FilterViewModel.Max, this.FilterViewModel.Filter,
                                             this.FilterViewModel.UseRegexp, this.FilterViewModel.ExcludeNoLevel)
                   ?? 0;
        }

        public string[] FetchRange(long offset, int limit)
        {
            var result = new string[limit];
            var ix = 0;
            this.store?.ReadMessages(limit, message => result[ix++] = this.CreateXaml(message), () => true, this.FilterViewModel.Start,
                                     this.FilterViewModel.Finish, offset,
                                     this.FilterViewModel.Reverse,
                                     this.FilterViewModel.Min, this.FilterViewModel.Max, this.FilterViewModel.Filter,
                                     this.FilterViewModel.UseRegexp);
            return result;
        }

        private string CreateRtf(LogMessage message)
        {
            var doc = new RtfDocument();
            var logLvel = LogLevel.None;
            if (this.store.HasLogLevelProperty)
            {
                logLvel = (LogLevel)message.IntegerProperty(this.store.LogLevelProperty);
            }

            doc.AddText(message.Header.Trim(), this.settings.FormatHead(logLvel));

            var txt = message.Body;
            doc.AddNewLine();
            if (string.IsNullOrWhiteSpace(txt))
            {
                return doc.Rtf;
            }

            doc.AddText(txt.Trim(), this.settings.FormatBody(logLvel));
            doc.AddNewLine();
            return doc.Rtf;
        }

        private const string RunElement = "Run";

        private const string LineBreakElement = "LineBreak";

        private const string FontFamilyAttr = "FontFamily";

        private const string FontWeightAttr = "FontWeight";

        private const string ForegroundAttr = "Foreground";

        private const string FontSizeAttr = "FontSize";

        private string CreateXaml(LogMessage message)
        {
            var sb = new StringBuilder();

            var logLvel = LogLevel.None;
            if (this.store.HasLogLevelProperty)
            {
                logLvel = (LogLevel)message.IntegerProperty(this.store.LogLevelProperty);
            }

            var format = this.settings.GetFormat(logLvel);

            using (var writer = XmlWriter.Create(sb, xmlWriterSettings))
            {
                WriteRunElement(message.Header.Trim(), writer, format.Font, format.ColorAsString, format.SizeHead, true);

                WriteLineBreak(writer);

                var txt = message.Body;

                if (!string.IsNullOrWhiteSpace(txt))
                {
                    WriteRunElement(txt.Trim(), writer, format.Font, format.ColorAsString, format.SizeBody, false);
                    WriteLineBreak(writer);
                }
            }

            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteLineBreak(XmlWriter writer)
        {
            writer.WriteElementString(LineBreakElement, string.Empty);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteRunElement(string text, XmlWriter writer, string font, string color, string size, bool bold)
        {
            writer.WriteStartElement(RunElement);
            writer.WriteAttributeString(FontFamilyAttr, font);
            if (bold)
            {
                writer.WriteAttributeString(FontWeightAttr, "Bold");
            }
            writer.WriteAttributeString(ForegroundAttr, color);
            writer.WriteAttributeString(FontSizeAttr, size);
            writer.WriteCData(text);
            writer.WriteEndElement();
        }
    }
}
