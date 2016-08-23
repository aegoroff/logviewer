using System.Collections.Generic;
using logviewer.logic.models;
using logviewer.logic.ui.main;

namespace logviewer.logic.ui
{
    public static class Extensions
    {
        public static IEnumerable<TemplateCommandViewModel> ToCommands(this IEnumerable<ParsingTemplate> templates, int selected)
        {
            var ix = 0;
            foreach (var t in templates)
            {
                yield return new TemplateCommandViewModel
                {
                    Text = t.DisplayName,
                    Checked = ix == selected,
                    Index = ix
                };
                ++ix;
            }
        }
    }
}