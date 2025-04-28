using ICSharpCode.AvalonEdit;
using System.Windows;

namespace PropGen.WPF.Behaviors
{
    public static class AvalonEditBehavior
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached(
                "Text",
                typeof(string),
                typeof(AvalonEditBehavior),
                new FrameworkPropertyMetadata(default(string),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    PropertyChangedCallback));

        public static string GetText(DependencyObject obj)
        {
            return (string)obj.GetValue(TextProperty);
        }

        public static void SetText(DependencyObject obj, string value)
        {
            obj.SetValue(TextProperty, value);
        }

        private static void PropertyChangedCallback(
            DependencyObject obj,
            DependencyPropertyChangedEventArgs args)
        {
            if (obj is TextEditor editor)
            {
                if (args.NewValue != null)
                {
                    var newValue = args.NewValue.ToString();
                    if (editor.Document != null && editor.Text != newValue)
                    {
                        editor.Document.Text = newValue;
                    }
                }

                // Cleanup previous event
                editor.TextChanged -= EditorOnTextChanged;

                // Subscribe to new event
                editor.TextChanged += EditorOnTextChanged;
            }
        }

        private static void EditorOnTextChanged(object? sender, EventArgs eventArgs)
        {
            if (sender is TextEditor editor)
            {
                SetText(editor, editor.Document.Text);
            }
        }
    }
}
