using System.Windows.Controls;
using System.Windows;

namespace PropGen.WPF.Behaviors
{
    public static class ScrollViewerBehavior
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached(
                "AutoScroll",
                typeof(bool),
                typeof(ScrollViewerBehavior),
                new PropertyMetadata(false, OnAutoScrollChanged));

        public static bool GetAutoScroll(DependencyObject obj) => (bool)obj.GetValue(AutoScrollProperty);
        public static void SetAutoScroll(DependencyObject obj, bool value) => obj.SetValue(AutoScrollProperty, value);

        private static void OnAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer && e.NewValue is bool shouldAutoScroll)
            {
                if (shouldAutoScroll)
                {
                    scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                }
                else
                {
                    scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
                }
            }
        }

        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            // Only auto-scroll if user is already at or near the bottom
            if (e.ExtentHeightChange > 0 && scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 1)
            {
                scrollViewer.ScrollToEnd();
            }
        }
    }
}
