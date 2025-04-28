using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using PropGen.WPF.Helpers;
using PropGen.WPF.ViewModels;

namespace PropGen.WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;

            DataContext = viewModel;
            viewModel.CloseAction = () => Close();

            // Hook into ErrorLines collection
            viewModel.ErrorLines.CollectionChanged += (_, _) => ApplyHighlighting(InputText);

            viewModel.Initialize(InputText, OutputText);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadAppData();  
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {            
            _viewModel.SaveAppData();            
        }

        private void InputText_Loaded(object sender, RoutedEventArgs e)
        {
            // Add a spacing margin between line numbers and text
            InputText.TextArea.LeftMargins.Add(new System.Windows.Controls.Border
            {
                Width = 10,
                Background = Brushes.Transparent 
            });
        }

        private void ApplyHighlighting(TextEditor textEditor)
        {         
            textEditor.TextArea.TextView.LineTransformers.Clear();
            textEditor.TextArea.TextView.LineTransformers.Add(new ErrorLineColorizer(_viewModel.ErrorLines));
            textEditor.TextArea.TextView.Redraw();            
        }               

        private void InputText_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (_viewModel.IsFileParser && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }
    }
}
