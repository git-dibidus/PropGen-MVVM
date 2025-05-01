using System.IO;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit;
using PropGen.Core.Models;
using PropGen.Core.Services;
using PropGen.WPF.Helpers;
using PropGen.WPF.Services;
using System.Printing;

namespace PropGen.WPF.ViewModels
{
    /// <summary>
    /// The main view model for the Property Generator application.
    /// Handles all business logic for the UI including:
    /// - Parsing input text or source files for property definitions
    /// - Generating property code based on user preferences
    /// - Managing application state and settings
    /// - Handling file operations (open/save)
    /// - Providing commands for UI interactions
    /// - Managing editor operations (copy/paste/cut/select)
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        #region Fields

        private string _className = string.Empty;
        private TextEditor? _inputEditor;
        private TextEditor? _outputEditor;

        private readonly IAppDataService _appDataService;
        private readonly IDialogService _dialogService;
        private readonly ITextParserService _textParserService;
        private readonly ISourceParserService _sourceParserService;
        private readonly IPropertyCodeGenerator _propertyCodeGenerator;

        #endregion

        #region Properties

        public Action? CloseAction { get; set; }
        public ObservableCollection<int> ErrorLines { get; } = new();

        [ObservableProperty]
        private ApplicationData appData = new();

        [ObservableProperty]
        private ObservableCollection<KeyValuePair<FieldNamingStyle, string>> namingStyleOptions;

        [ObservableProperty]
        private KeyValuePair<FieldNamingStyle, string> selectedNamingStyle;

        [ObservableProperty]
        private bool isFileParser = false;

        [ObservableProperty]
        private string inputText = string.Empty;

        [ObservableProperty]
        private string generatedCode = string.Empty;

        [ObservableProperty]
        private string statusText = string.Empty;

        [ObservableProperty]
        private string modeText = string.Empty;

        [ObservableProperty]
        private bool isSaveFileEnabled = false;

        [ObservableProperty]
        private bool isCopiedToClipboardVisible = false;

        #endregion

        #region Constructors

        public MainViewModel(
            ITextParserService textParserService,
            ISourceParserService sourceParserService,
            IPropertyCodeGenerator propertyCodeGenerator,
            IAppDataService appDataService,
            IDialogService dialogService)
        {
            _appDataService = appDataService;
            _textParserService = textParserService;
            _sourceParserService = sourceParserService;
            _propertyCodeGenerator = propertyCodeGenerator;
            _dialogService = dialogService;

            NamingStyleOptions = GetFieldNamingStyles();
        }

        #endregion

        #region Initialization

        public void Initialize(TextEditor inputEditor, TextEditor outputEditor)
        {
            _inputEditor = inputEditor;
            _outputEditor = outputEditor;

            _inputEditor.TextArea.SelectionChanged += (s, e) => RaiseAllCanExecuteInputChanged();
            _inputEditor.TextChanged += (s, e) => RaiseAllCanExecuteInputChanged();

            _outputEditor.TextArea.SelectionChanged += (s, e) => RaiseAllCanExecuteOutputChanged();
            _outputEditor.TextChanged += (s, e) => RaiseAllCanExecuteOutputChanged();
        }

        #endregion

        #region Property Changed Handlers

        partial void OnIsFileParserChanged(bool value) => ApplyModeChange(value);

        partial void OnSelectedNamingStyleChanged(KeyValuePair<FieldNamingStyle, string> value)
        {
            if (value.Key != AppData.Options.FieldNamingStyle)
            {
                AppData.Options.FieldNamingStyle = value.Key;
            }
        }

        #endregion

        #region Command Methods

        [RelayCommand(CanExecute = nameof(CanCopyInput))]
        private void CopyInput() => _inputEditor?.Copy();

        [RelayCommand(CanExecute = nameof(CanPasteInput))]
        private void PasteInput() => _inputEditor?.Paste();

        [RelayCommand(CanExecute = nameof(CanCutInput))]
        private void CutInput() => _inputEditor?.Cut();

        [RelayCommand(CanExecute = nameof(CanSelectAllInput))]
        private void SelectAllInput() => _inputEditor?.SelectAll();

        [RelayCommand(CanExecute = nameof(CanCopyOutput))]
        private void CopyOutput() => _outputEditor?.Copy();

        [RelayCommand(CanExecute = nameof(CanSelectAllOutput))]
        private void SelectAllOutput() => _outputEditor?.SelectAll();

        [RelayCommand(CanExecute = nameof(CanGenerate))]
        private void Generate()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                _dialogService.ShowMessage("Input cannot be empty.", MessageType.Warning);
                return;
            }

            if (AppData.Options.FieldNamingStyle == FieldNamingStyle.PrefixedCamelCase &&
                string.IsNullOrWhiteSpace(AppData.Options.FieldPrefix))
            {
                _dialogService.ShowMessage("Field Prefix must be specified if 'Prefixed Camel Case' selected.", MessageType.Warning);
                return;
            }

            try
            {
                StatusText = string.Empty;
                ErrorLines.Clear();

                PropertyParserResult result = IsFileParser ?
                    _sourceParserService.ParseSource(InputText) :
                    _textParserService.ParseText(InputText);

                if (result.HasIssues)
                {
                    var sb = new StringBuilder();
                    foreach (var issue in result.Issues)
                    {
                        ErrorLines.Add(issue.LineNumber);
                        string message = (issue.Severity == IssueSeverity.Warning ? "Warning" : "ERROR") + $" - Line {issue.LineNumber}: {issue.Message}";
                        sb.AppendLine(message);
                    }
                    StatusText = sb.ToString();
                }

                if (result.Classes.Count == 0 || result.Classes[0]?.Properties.Count == 0)
                {
                    StatusText = "WARNING: No eligible Classes/Properties found";
                    return;
                }

                GeneratedCodeResult genResult = _propertyCodeGenerator.GenerateProperties(result.Classes, AppData.Options);
                GeneratedCode = genResult.Content;

                if (genResult.PropertyCount > 0)
                {
                    if (!string.IsNullOrEmpty(StatusText))
                        StatusText += Environment.NewLine;

                    string issuesMessage = result.HasIssues ? " (with issues)" : "";
                    StatusText += $"Generated {genResult.PropertyCount} properties{issuesMessage}.";

                    IsSaveFileEnabled = IsFileParser;
                    _className = genResult.Classes.First().ClassName;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, MessageType.Error);
            }
        }

        [RelayCommand]
        private async Task HandleDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files == null || files.Length == 0) return;

                if (files.Length > 1)
                {
                    _dialogService.ShowMessage("Only one file can be dropped at a time.", MessageType.Warning);
                    return;
                }

                string filePath = files[0];
                try
                {
                    string extension = Path.GetExtension(filePath).ToLower();
                    if (extension != ".cs")
                    {
                        _dialogService.ShowMessage("Only C# files are supported.", MessageType.Warning);
                        return;
                    }

                    InputText = await File.ReadAllTextAsync(filePath);
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessage($"Error loading file: {ex.Message}", MessageType.Error);
                }
            }
        }

        [RelayCommand]
        private void ClearAll()
        {
            if (!_dialogService.AskYesNoDialog("Are you sure you want to clear the workspace?", "Clear All"))
            {
                return;
            }

            ClearAllWithoutConfirmation();
        }

        [RelayCommand]
        private void Close()
        {
            CloseAction?.Invoke();
        }

        [RelayCommand]
        private void OpenFile()
        {
            string? filePath = _dialogService.OpenFile("C# Files (*.cs)|*.cs");
            if (filePath != null)
            {
                try
                {
                    InputText = File.ReadAllText(filePath);
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessage($"Error loading file: {ex.Message}", MessageType.Error);
                }
            }
        }

        [RelayCommand]
        private void SaveFile()
        {
            string fileName = string.IsNullOrEmpty(_className)
                ? "GeneratedProperties.cs"
                : $"{_className}.mvvm.cs";

            string? filePath = _dialogService.SaveFile("C# Files (*.cs)|*.cs", fileName);
            if (filePath != null)
            {
                try
                {
                    File.WriteAllText(filePath, GeneratedCode);
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessage($"Error saving file: {ex.Message}", MessageType.Error);
                }
            }
        }

        [RelayCommand]
        private void CopyToClipboard()
        {
            try
            {
                Clipboard.SetText(GeneratedCode);
                IsCopiedToClipboardVisible = true;
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Error copying to clipboard: {ex.Message}", MessageType.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanSortLines))]
        private void SortLines()
        {            
            try
            {
                InputText = LineSortHelper.SortLines(InputText);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Error sorting lines: {ex.Message}", MessageType.Error);
            }
        }

        #endregion

        #region Private Methods

        private bool CanCopyInput() => _inputEditor != null && !_inputEditor.TextArea.Selection.IsEmpty;
        private bool CanPasteInput() => Clipboard.ContainsText();
        private bool CanCutInput() => _inputEditor != null && !_inputEditor.TextArea.Selection.IsEmpty;
        private bool CanSelectAllInput() => _inputEditor != null && !string.IsNullOrWhiteSpace(_inputEditor.Text);
        private bool CanCopyOutput() => _outputEditor != null && !_outputEditor.TextArea.Selection.IsEmpty;
        private bool CanSelectAllOutput() => _outputEditor != null && !string.IsNullOrWhiteSpace(_outputEditor.Text);
        private bool CanGenerate() => _inputEditor != null && !string.IsNullOrWhiteSpace(_inputEditor.Text);
        private bool CanSortLines() => _inputEditor != null && LineSortHelper.HasMoreThanOneLine(_inputEditor.Text) && !IsFileParser;

        private void RaiseAllCanExecuteInputChanged()
        {
            CopyInputCommand.NotifyCanExecuteChanged();
            PasteInputCommand.NotifyCanExecuteChanged();
            CutInputCommand.NotifyCanExecuteChanged();
            SelectAllInputCommand.NotifyCanExecuteChanged();
            GenerateCommand.NotifyCanExecuteChanged();
            SortLinesCommand.NotifyCanExecuteChanged();
        }

        private void RaiseAllCanExecuteOutputChanged()
        {
            CopyOutputCommand.NotifyCanExecuteChanged();
            SelectAllOutputCommand.NotifyCanExecuteChanged();
        }

        private void ClearAllWithoutConfirmation()
        {
            InputText = string.Empty;
            GeneratedCode = string.Empty;
            StatusText = string.Empty;
            ErrorLines.Clear();
            IsSaveFileEnabled = false;
            IsCopiedToClipboardVisible = false;
            _className = string.Empty;
        }

        private void ApplyModeChange(bool isFileMode)
        {
            ModeText = isFileMode ? "File Input Mode" : "Text Input Mode";
            ClearAllWithoutConfirmation();
            AppData.IsFileParser = isFileMode;
        }

        private ObservableCollection<KeyValuePair<FieldNamingStyle, string>> GetFieldNamingStyles()
        {
            return new ObservableCollection<KeyValuePair<FieldNamingStyle, string>>
            {
                new(FieldNamingStyle.UnderscoreCamelCase, "Underscore Camel Case"),
                new(FieldNamingStyle.CamelCase, "Camel Case"),
                new(FieldNamingStyle.PrefixedCamelCase, "Prefixed Camel Case")
            };
        }

        #endregion

        #region App Data Methods

        public void LoadAppData()
        {
            try
            {
                AppData = _appDataService.Load();
                IsFileParser = AppData.IsFileParser;
                SelectedNamingStyle = NamingStyleOptions.Single(x => x.Key == AppData.Options.FieldNamingStyle);
                ApplyModeChange(IsFileParser);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Error loading App Settings file\n{ex}", MessageType.Error);
            }
        }

        public void SaveAppData()
        {
            try
            {
                _appDataService.Save(AppData);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Error saving App Settings file\n{ex}", MessageType.Error);
            }
        }

        #endregion
    }
}