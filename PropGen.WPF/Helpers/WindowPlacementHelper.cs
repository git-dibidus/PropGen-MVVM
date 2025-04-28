using System.Windows.Forms;
using System.Windows.Interop;
using System.Drawing;

namespace PropGen.WPF.Helpers
{
    public static class WindowPlacementHelper
    {
        public static void ApplySafeWindowPlacement(WindowLocationAndSize saved)
        {
            //var windowInteropHelper = new WindowInteropHelper(window);
            var screens = Screen.AllScreens;

            // Build a rectangle from saved values
            var savedRect = new Rectangle((int)saved.Left, (int)saved.Top, (int)saved.Width, (int)saved.Height);

            // Check if it intersects any screen
            bool isVisible = false;
            foreach (var screen in screens)
            {
                if (screen.WorkingArea.IntersectsWith(savedRect))
                {
                    isVisible = true;
                    break;
                }
            }

            if (!isVisible)
            {
                // Saved position is completely off-screen, reset to primary screen                
                if (Screen.PrimaryScreen == null)
                    return; 

                var primaryScreen = Screen.PrimaryScreen.WorkingArea;

                saved.Left = primaryScreen.Left + 50;
                saved.Top = primaryScreen.Top + 50;
                saved.Width = Math.Min(saved.Width, primaryScreen.Width - 100);
                saved.Height = Math.Min(saved.Height, primaryScreen.Height - 100);
            }
            else
            {
                // Ensure it does not overflow current screen bounds
                foreach (var screen in screens)
                {
                    if (screen.WorkingArea.IntersectsWith(savedRect))
                    {
                        double left = saved.Left;
                        double top = saved.Top;
                        double width = saved.Width;
                        double height = saved.Height;

                        if (left < screen.WorkingArea.Left)
                            left = screen.WorkingArea.Left;

                        if (top < screen.WorkingArea.Top)
                            top = screen.WorkingArea.Top;

                        if (left + width > screen.WorkingArea.Right)
                            left = screen.WorkingArea.Right - width;

                        if (top + height > screen.WorkingArea.Bottom)
                            top = screen.WorkingArea.Bottom - height;

                        // Optionally shrink if bigger than screen
                        width = Math.Min(width, screen.WorkingArea.Width);
                        height = Math.Min(height, screen.WorkingArea.Height);

                        saved.Width = width;
                        saved.Height = height;

                        saved.Left = left;
                        saved.Top = top;
                        break;
                    }
                }
            }
        }
    }
}
