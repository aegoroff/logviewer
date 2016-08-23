using logviewer.logic.Annotations;

namespace logviewer.logic.ui.main
{
    [PublicAPI]
    public class TemplateCommandViewModel
    {
        public string Text { get; set; }
        public bool Checked { get; set; }
        public int Index { get; set; }
    }
}