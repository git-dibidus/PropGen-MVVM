using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace PropGen.WPF.Helpers
{
    public class ErrorLineColorizer : DocumentColorizingTransformer
    {
        private readonly HashSet<int> _highlightedLines;

        public ErrorLineColorizer(IEnumerable<int> lines)
        {
            _highlightedLines = new HashSet<int>(lines);
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (_highlightedLines.Contains(line.LineNumber))
            {
                ChangeLinePart(line.Offset, line.EndOffset, el =>
                {
                    el.TextRunProperties.SetBackgroundBrush(Brushes.MistyRose);
                    el.TextRunProperties.SetForegroundBrush(Brushes.DarkRed);
                });
            }
        }
    }
}
