using System.Windows.Input;
using System.Windows;

namespace PropGen.WPF.Helpers
{
    public static class DropCommandHelper
    {
        public static readonly DependencyProperty DropCommandProperty =
            DependencyProperty.RegisterAttached(
                "DropCommand",
                typeof(ICommand),
                typeof(DropCommandHelper),
                new PropertyMetadata(null, OnDropCommandChanged));

        public static ICommand GetDropCommand(DependencyObject obj) => (ICommand)obj.GetValue(DropCommandProperty);
        public static void SetDropCommand(DependencyObject obj, ICommand value) => obj.SetValue(DropCommandProperty, value);

        private static void OnDropCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement uiElement)
            {
                uiElement.Drop -= UIElement_Drop;
                if (e.NewValue is ICommand)
                {
                    uiElement.Drop += UIElement_Drop;
                }
            }
        }

        private static void UIElement_Drop(object sender, DragEventArgs e)
        {
            if (sender is DependencyObject d)
            {
                ICommand command = GetDropCommand(d);
                if (command != null && command.CanExecute(e))
                {
                    command.Execute(e);
                }
            }
        }
    }
}
