namespace PropGen.WPF.Services
{
    /// <summary>
    /// Provides simplified message box display functionality with standardized styling.
    /// </summary>
    public static class MsgBox
    {
        public static void Show(string message, MessageType type)
        {
            string caption = string.Empty;

            switch (type)
            {
                case MessageType.Error:
                    caption = "Error";
                    break;
                case MessageType.Warning:
                    caption = "Warning";
                    break;
                case MessageType.Information:
                    caption = "Info";
                    break;
            }

            Show(message, type, caption);
        }

        public static void Show(string message, MessageType type, string caption)
        {
            switch (type)
            {
                case MessageType.Error:
                    System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    break;
                case MessageType.Warning:
                    System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    break;
                case MessageType.Information:
                    System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    break;
            }
        }
    }

    public enum MessageType
    {
        Error,
        Warning,
        Information
    }
}
