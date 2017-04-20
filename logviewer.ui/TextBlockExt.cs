// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 20.04.2017
// © 2012-2017 Alexander Egorov

using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace logviewer.ui
{
    public class TextBlockExt : TextBlock
    {
        public string SelectedText = "";

        public delegate void TextSelectedHandler(string selectedText);

        public event TextSelectedHandler OnTextSelected;

        protected void RaiseEvent() => this.OnTextSelected?.Invoke(this.SelectedText);

        private TextPointer startSelectPosition;

        private TextPointer endSelectPosition;

        private Brush saveForeGroundBrush;

        private Brush saveBackGroundBrush;

        private TextRange ntr;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (this.ntr != null)
            {
                this.ntr.ApplyPropertyValue(TextElement.ForegroundProperty, this.saveForeGroundBrush);
                this.ntr.ApplyPropertyValue(TextElement.BackgroundProperty, this.saveBackGroundBrush);
            }

            var mouseDownPoint = e.GetPosition(this);
            this.startSelectPosition = this.GetPositionFromPoint(mouseDownPoint, true);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            var mouseUpPoint = e.GetPosition(this);
            this.endSelectPosition = this.GetPositionFromPoint(mouseUpPoint, true);

            if (this.startSelectPosition == null || this.endSelectPosition == null)
            {
                return;
            }

            this.ntr = new TextRange(this.startSelectPosition, this.endSelectPosition);

            // keep saved
            this.saveForeGroundBrush = (Brush)this.ntr.GetPropertyValue(TextElement.ForegroundProperty);
            this.saveBackGroundBrush = (Brush)this.ntr.GetPropertyValue(TextElement.BackgroundProperty);

            // change style
            this.ntr.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.DarkGray));
            this.ntr.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.DarkBlue));

            this.SelectedText = this.ntr.Text;

            this.Cursor = Cursors.IBeam;
        }
    }
}
