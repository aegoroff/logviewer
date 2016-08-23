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