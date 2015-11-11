// Created by: egr
// Created at: 10.11.2015
// © 2012-2015 Alexander Egorov

using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace logviewer.ui
{
    public class RichTextBoxHelper : DependencyObject
    {
        public static readonly DependencyProperty DocumentRtfProperty =
            DependencyProperty.RegisterAttached(
                "DocumentRtf",
                typeof (string),
                typeof (RichTextBoxHelper),
                new FrameworkPropertyMetadata
                {
                    BindsTwoWayByDefault = false,
                    PropertyChangedCallback = (obj, e) =>
                    {
                        var richTextBox = (RichTextBox) obj;

                        // Parse the XAML to a document (or use XamlReader.Parse())
                        var xaml = GetDocumentRtf(richTextBox);
                        if (string.IsNullOrWhiteSpace(xaml))
                        {
                            return;
                        }
                        var doc = new FlowDocument();
                        var range = new TextRange(doc.ContentStart, doc.ContentEnd);

                        range.Load(new MemoryStream(Encoding.ASCII.GetBytes(xaml)), DataFormats.Rtf);

                        // Set the document
                        richTextBox.Document = doc;

                        // When the document changes update the source
                        range.Changed += (obj2, e2) =>
                        {
                            if (richTextBox.Document == doc)
                            {
                                var buffer = new MemoryStream();
                                range.Save(buffer, DataFormats.Rtf);
                                SetDocumentRtf(richTextBox,
                                    Encoding.ASCII.GetString(buffer.ToArray()));
                            }
                        };
                    }
                });

        public static string GetDocumentRtf(DependencyObject obj)
        {
            return (string) obj.GetValue(DocumentRtfProperty);
        }

        public static void SetDocumentRtf(DependencyObject obj, string value)
        {
            obj.SetValue(DocumentRtfProperty, value);
        }
    }
}