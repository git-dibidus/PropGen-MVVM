using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace PropGen.WPF.Services
{
    public class DialogService(IServiceProvider serviceProvider) : IDialogService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public void ShowDialog<T>(Window? owner = null, object? parameters = null) where T : Window
        {
            _ = ShowDialogWithResult<T, object>(owner, parameters);
        }

        public TResult? ShowDialogWithResult<T, TResult>(Window? owner = null, object? parameters = null) where T : Window
        {
            var dialog = _serviceProvider.GetRequiredService<T>();

            dialog.Owner = owner ?? Application.Current.MainWindow;

            // Pass parameters if the dialog supports it
            if (dialog is IDialogParameterizable parameterizableDialog && parameters != null)
            {
                parameterizableDialog.SetParameters(parameters);
            }

            // Show the dialog and check if it's capable of returning a result
            bool? dialogResult = dialog.ShowDialog();

            if (dialogResult == true && dialog is IDialogResultProvider<TResult> resultProvider)
            {
                return resultProvider.GetResult();
            }

            return default;
        }

        public bool AskYesNoDialog(string message, string title)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public void ShowMessage(string message, MessageType type)
        {
            MsgBox.Show(message, type);
        }

        public void ShowMessage(string message, MessageType type, string caption)
        {
            MsgBox.Show(message, type, caption);
        }
    }
}
