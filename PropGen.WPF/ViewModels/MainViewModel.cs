using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PropGen.Core.Models;
using PropGen.Core.Services;
using PropGen.WPF.Services;
using PropGen.WPF.Helpers;
using ICSharpCode.AvalonEdit;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace PropGen.WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public Action? CloseAction { get; set; }

        [ObservableProperty]
        private ApplicationData appData = new ApplicationData();

        [ObservableProperty]
        private ObservableCollection<KeyValuePair<FieldNamingStyle, string>> namingStyleOptions;

        [ObservableProperty]
        private KeyValuePair<FieldNamingStyle, string> selectedNamingStyle;

        public ObservableCollection<int> ErrorLines { get; } = new();
        
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
        
        partial void OnIsFileParserChanged(bool value) => ApplyModeChange(value);
        partial void OnSelectedNamingStyleChanged(KeyValuePair<FieldNamingStyle, string> value)
        {
            if (value.Key != AppData.Options.FieldNamingStyle)
            {
                AppData.Options.FieldNamingStyle = value.Key;
            }
        }        

        private ICSharpCode.AvalonEdit.TextEditor? _inputEditor;
        private ICSharpCode.AvalonEdit.TextEditor? _outputEditor;

        private readonly IAppDataService _appDataService;
        private readonly IDialogService _dialogService;
        private readonly ITextParserService _textParserService;
        private readonly ISourceParserService _sourceParserService;
        private readonly IPropertyCodeGenerator _propertyCodeGenerator;

        public MainViewModel(ITextParserService textParserService,
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

        public void Initialize(TextEditor inputEditor, TextEditor outputEditor)
        {
            _inputEditor = inputEditor;
            _outputEditor = outputEditor;

            _inputEditor.TextArea.SelectionChanged += (s, e) => RaiseAllCanExecuteInputChanged();
            _inputEditor.TextChanged += (s, e) => RaiseAllCanExecuteInputChanged();

            _outputEditor.TextArea.SelectionChanged += (s, e) => RaiseAllCanExecuteOutputChanged();
            _outputEditor.TextChanged += (s, e) => RaiseAllCanExecuteOutputChanged();
        }

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


        private bool CanCopyInput() => _inputEditor != null && !_inputEditor.TextArea.Selection.IsEmpty;
        private bool CanPasteInput() => Clipboard.ContainsText();
        private bool CanCutInput() => _inputEditor != null && !_inputEditor.TextArea.Selection.IsEmpty;
        private bool CanSelectAllInput() => _inputEditor != null && !string.IsNullOrWhiteSpace(_inputEditor.Text);

        private bool CanCopyOutput() => _outputEditor != null && !_outputEditor.TextArea.Selection.IsEmpty;
        private bool CanSelectAllOutput() => _outputEditor != null && !string.IsNullOrWhiteSpace(_outputEditor.Text);

        private void RaiseAllCanExecuteInputChanged()
        {
            CopyInputCommand.NotifyCanExecuteChanged();
            PasteInputCommand.NotifyCanExecuteChanged();
            CutInputCommand.NotifyCanExecuteChanged();
            SelectAllInputCommand.NotifyCanExecuteChanged();
        }

        private void RaiseAllCanExecuteOutputChanged()
        {
            CopyOutputCommand.NotifyCanExecuteChanged();
            SelectAllOutputCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private void Generate()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                _dialogService.ShowMessage("Input cannot be empty.", MessageType.Warning);
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

                if (result.Classes[0].Properties.Count == 0)
                    return;


                //int propertyCount = 0;
                //var res = new StringBuilder();

                //foreach (var classItem in result.Classes)
                //{
                //    GeneratedCodeResult genResult = _propertyCodeGenerator.GenerateProperties(classItem, AppData.Options);
                //    res.AppendLine(genResult.Content);
                //    propertyCount += genResult.ClassInfo.Properties.Count;
                //}

                GeneratedCodeResult genResult = _propertyCodeGenerator.GenerateProperties(result.Classes, AppData.Options);

                GeneratedCode = genResult.Content;

                if (genResult.PropertyCount > 0)
                {
                    if (!string.IsNullOrEmpty(StatusText))
                        StatusText += Environment.NewLine;

                    string issuesMessage = result.HasIssues ? " (with issues)" : "";

                    StatusText += $"Generated {genResult.PropertyCount} properties{issuesMessage}.";
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
                if (files != null && files.Length > 0)
                {
                    if (files.Length > 1)
                    {
                        _dialogService.ShowMessage("Only one file can be dropped at a time.", MessageType.Warning);
                        return;
                    }

                    string filePath = files[0];
                    try
                    {
                        // Check if the file is a C# file
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
        }

        [RelayCommand]
        private void Close()
        {
            CloseAction?.Invoke();
        }

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

        private void ApplyModeChange(bool isFileMode)
        {
            ModeText = isFileMode ? "File Input Mode" : "Text Input Mode";

            InputText = string.Empty;
            GeneratedCode = string.Empty;
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
    }
}
