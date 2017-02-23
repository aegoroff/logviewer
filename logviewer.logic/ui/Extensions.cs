// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 28.08.2016
// © 2012-2017 Alexander Egorov

using System.Collections.Generic;
using System.Linq;
using logviewer.logic.models;
using logviewer.logic.ui.main;

namespace logviewer.logic.ui
{
    public static class Extensions
    {
        public static IEnumerable<TemplateCommandViewModel> ToCommands(this IEnumerable<ParsingTemplate> templates, int selected)
        {
            var ix = 0;
            return from t in templates select CreateTemplateCommand(selected, t, ix++);
        }

        private static TemplateCommandViewModel CreateTemplateCommand(int selected, ParsingTemplate template, int ix)
        {
            return new TemplateCommandViewModel
            {
                Text = template.DisplayName,
                Checked = ix == selected,
                Index = ix
            };
        }
    }
}