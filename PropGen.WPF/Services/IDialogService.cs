using System.Windows;

namespace PropGen.WPF.Services
{
    public interface IDialogService
    {
        void ShowDialog<T>(Window? owner = null, object? parameters = null) where T : Window;
        TResult? ShowDialogWithResult<T, TResult>(Window? owner = null, object? parameters = null) where T : Window;
        bool AskYesNoDialog(string message, string title);
        void ShowMessage(string message, MessageType type);
        void ShowMessage(string message, MessageType type, string caption);
    }
}
