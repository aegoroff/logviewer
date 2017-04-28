// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 28.08.2016
// © 2012-2017 Alexander Egorov

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using logviewer.logic.models;
using logviewer.logic.support;
using logviewer.logic.ui.main;

namespace logviewer.logic.ui
{
    public static class Extensions
    {
        public static IEnumerable<TemplateCommandViewModel> ToCommands(this IEnumerable<ParsingTemplate> templates, int selected)
        {
            var ix = 0;

            TemplateCommandViewModel CreateTemplateCommand(ParsingTemplate template)
            {
                return new TemplateCommandViewModel
                       {
                           Text = template.DisplayName,
                           Checked = ix == selected,
                           Index = ix++
                       };
            }

            return from t in templates select CreateTemplateCommand(t);
        }

        private static readonly XmlReaderSettings xmlReaderSettings = new XmlReaderSettings
        {
                                                                          ConformanceLevel = ConformanceLevel.Fragment,
                                                                          CheckCharacters = false
                                                                      };

        public static string CleanupXaml(this string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
            {
                return xaml;
            }

            string Cleanup()
            {
                var stringReader = new StringReader(xaml);

                var reader = XmlReader.Create(stringReader, xmlReaderSettings);
                var stringBuilder = new StringBuilder();
                using (reader)
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && string.Equals(reader.Name, "Run", StringComparison.Ordinal))
                        {
                            stringBuilder.AppendLine(reader.ReadElementContentAsString());
                        }
                    }
                }
                return stringBuilder.ToString();
            }

            try
            {
                return Cleanup();
            }
            catch (Exception e)
            {
                Log.Instance.Error(e.Message, e);
            }

            return xaml;
        }
    }
}
