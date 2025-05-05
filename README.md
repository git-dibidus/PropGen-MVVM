# MVVM Property Generator

A powerful tool for C# developers to quickly generate MVVM-compatible properties with `INotifyPropertyChanged` implementation. This application saves time and reduces boilerplate code when working with the MVVM pattern in WPF, MAUI, or other XAML-based frameworks.

![Application Screenshot](/Images/PropGen_text.png)

![Application Screenshot](/Images/PropGen_file.png)

## Features

- **Dual Input Modes**:
  - **Text Input Mode**: Quickly type properties in "Name, Type" or "Type Name" format
  - **File Input Mode**: Process existing C# class files with full namespace support

- **Smart Property Generation**:
  - Generates complete property implementations with `INotifyPropertyChanged`
  - Creates backing fields and `SetProperty` helper method
  - Maintains original class structure and namespace

- **Convenient File Operations**:
  - Drag-and-drop support for class files
  - Save generated output directly to .cs files
  - Preserves original file location when saving

- **Clean Code Generation**:
  - Properly formatted output with correct indentation
  - Includes auto-generated header comment
  - Follows C# coding conventions

## Installation

1. Download the latest release from https://github.com/git-dibidus/PropGen-MVVM/releases/tag/V1.0.0 
3. Follow the installation wizard instructions

## Usage

### Text Input Mode
1. Enter properties in the editor, one per line, in either format:
   - `PropertyName, PropertyType`
   - `PropertyType PropertyName`
2. Click "Generate" to create the MVVM properties
3. Copy output to Clipboard

### File Input Mode
1. Click "Open File" or drag-drop a C# class file into the application
2. The application will parse the existing class structure
3. Generated output will include the original namespace and class name
4. Use "Save As" to save the enhanced class to a new file (or Copy to Clipboard)

## Requirements

- .NET 8.0 or later
- Windows 10/11

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request with your improvements.

## License

[MIT License](LICENSE)

