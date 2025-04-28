using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PropGen.Core.Models;

namespace PropGen.Core.Services
{
    public class PropertyCodeGenerator : IPropertyCodeGenerator
    {
        private CodeGenerationOptions _options;

        public GeneratedCodeResult GenerateProperties(IEnumerable<ClassInfo> classes, CodeGenerationOptions options)
        {
            _options = options;

            var sb = new StringBuilder();
            var namespaces = new HashSet<string>();

            string codeText = string.Empty;
            var generatedClasses = new List<ClassInfo>();

            string namespaceName = string.Empty;
            bool isNamespaceAdded = false;
            bool isHeaderAdded = false;
            bool isUsingsAdded = false;
            int classCount = 1;

            foreach (var classInfo in classes)
            {
                bool hasNamespace = !string.IsNullOrEmpty(classInfo.Namespace);
                bool hasClass = !string.IsNullOrEmpty(classInfo.ClassName);

                if (hasNamespace && !namespaces.Contains(classInfo.Namespace))
                {
                    namespaces.Add(classInfo.Namespace);
                    namespaceName = classInfo.Namespace;
                    isNamespaceAdded = false;
                    isUsingsAdded = false;
                    isHeaderAdded = false;
                    classCount = classes.Count(c => c.Namespace == classInfo.Namespace);
                }

                // Add header
                if (hasClass && !isHeaderAdded)
                {
                    sb.AppendLine(AddHeader());
                    isHeaderAdded = true;
                }

                // Add usings
                if (hasClass && (_options.ImplementNotificationInterface || options.IsMvvmToolkitStyle) && !isUsingsAdded)
                {
                    if (_options.IsMvvmToolkitStyle)
                    {
                        sb.AppendLine("using CommunityToolkit.Mvvm.ComponentModel;");
                    }
                    else
                    {
                        sb.AppendLine("using System.ComponentModel;");
                        sb.AppendLine("using System.Runtime.CompilerServices;");
                    }

                    sb.AppendLine();
                    isUsingsAdded = true;
                }

                // Add namespace if provided
                if (hasNamespace && !isNamespaceAdded)
                {
                    sb.AppendLine($"namespace {classInfo.Namespace}");
                    sb.AppendLine("{");
                    isNamespaceAdded = true;
                }

                // Generate class declaration if class name provided
                if (hasClass)
                {
                    var indent = hasNamespace ? "    " : "";
                    sb.AppendLine($"{indent}{GetClassDeclaration(classInfo)}");
                    sb.AppendLine($"{indent}{{");
                }

                // Determine indentation level
                var propertyIndent = (hasNamespace ? "    " : "") + (hasClass ? "    " : "");

                // Generate INotifyPropertyChanged event and method
                if (hasClass && options.ImplementNotificationInterface && !options.IsMvvmToolkitStyle)
                {
                    if (!options.IsCompactStyle)
                    {
                        // Generate event declaration
                        sb.AppendLine($"{propertyIndent}public event {options.NotificationInterfaceName}.PropertyChangedEventHandler PropertyChanged;");
                        sb.AppendLine();

                        // Generate property changed method
                        sb.AppendLine(GenerateOnPropertyChangedMethod(propertyIndent));
                    }
                    else
                    {
                        // Generate SetProperty helper method for compact style
                        sb.AppendLine(GenerateSetPropertyMethod(propertyIndent));
                    }
                }

                // Generate properties
                int propertyCount = classInfo.Properties.Count;
                foreach (var property in classInfo.Properties)
                {
                    if (options.WrapInRegions)
                    {
                        sb.AppendLine($"{propertyIndent}#region Property {property.Name}");
                    }

                    sb.Append(GenerateProperty(property, propertyIndent));

                    if (options.WrapInRegions)
                    {
                        sb.AppendLine($"{propertyIndent}#endregion");
                    }

                    propertyCount--;

                    if (propertyCount > 0)
                    {
                        sb.AppendLine();
                    }
                }

                // Close class if opened
                if (hasClass)
                {
                    var indent = hasNamespace ? "    " : "";
                    sb.AppendLine($"{indent}}}");
                }

                classCount--;

                // Close namespace if opened and all classes in the namespace are processed
                if (hasNamespace && classCount == 0)
                {
                    sb.AppendLine("}");
                }
                else
                {
                    sb.AppendLine();
                }

                codeText += sb.ToString();
                sb.Clear();
                generatedClasses.Add(classInfo);
            }

            return new GeneratedCodeResult
            {
                Content = codeText,
                Classes = generatedClasses,
                PropertyCount = generatedClasses.Sum(c => c.Properties.Count)
            };
        }               

        private string GetClassDeclaration(ClassInfo classInfo)
        {
            string modifier = _options.IsMvvmToolkitStyle ? "public partial" : "public";
            string className = $"{modifier} class {classInfo.ClassName}";

            if (_options.IsMvvmToolkitStyle)
            {
                className += " : ObservableObject";
            }
            else
            {
                if (_options.ImplementNotificationInterface)
                {
                    className += $" : {_options.NotificationInterfaceName}";
                }
            }

            return className;
        }

        private string GetMvvmToolkitClassDeclaration(ClassInfo classInfo)
        {
            return $"public partial class {classInfo.ClassName} : ObservableObject";
        }

        private string AddHeader()
        {
            var sb = new StringBuilder();

            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("//     This code was generated by PropGen MVVM tool.");
            sb.AppendLine("// </auto-generated>");

            return sb.ToString();
        }

        private string GenerateOnPropertyChangedMethod(string indent)
        {
            var methodName = "OnPropertyChanged";

            var sb = new StringBuilder();

            if (_options.UseCallerMemberName)
            {
                sb.AppendLine($"{indent}protected virtual void {methodName}([CallerMemberName] string propertyName = null)");
            }
            else
            {
                sb.AppendLine($"{indent}protected virtual void {methodName}(string propertyName)");
            }

            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));");
            sb.AppendLine($"{indent}}}");
            return sb.ToString();
        }

        private string GenerateSetPropertyMethod(string indent)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{indent}protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (EqualityComparer<T>.Default.Equals(field, value)) return false;");
            sb.AppendLine($"{indent}    field = value;");
            sb.AppendLine($"{indent}    OnPropertyChanged(propertyName);");
            sb.AppendLine($"{indent}    return true;");
            sb.AppendLine($"{indent}}}");
            sb.AppendLine();
            sb.AppendLine($"{indent}public event PropertyChangedEventHandler PropertyChanged;");
            sb.AppendLine();
            sb.AppendLine($"{indent}protected void OnPropertyChanged([CallerMemberName] string propertyName = null)");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));");
            sb.AppendLine($"{indent}}}");

            return sb.ToString();
        }

        private string GenerateProperty(PropertyInfo property, string indent)
        {
            if (_options.IsMvvmToolkitStyle)
            {
                return GenerateMvvmToolkitProperty(property, indent);
            }

            if (_options.IsCompactStyle)
            {
                return GenerateCompactProperty(property, indent);
            }
            return GenerateFullProperty(property, indent);
        }


        private string GenerateCompactProperty(PropertyInfo property, string indent)
        {
            var fieldName = GetFieldName(property.Name);
            var sb = new StringBuilder();

            // Field declaration
            sb.AppendLine($"{indent}private {property.Type} {fieldName};");

            // Property declaration
            sb.AppendLine($"{indent}public {property.Type} {property.Name}");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    get => {fieldName};");

            if (property.HasSetter)
            {
                sb.AppendLine($"{indent}    set => SetProperty(ref {fieldName}, value);");
            }

            sb.AppendLine($"{indent}}}");

            return sb.ToString();
        }

        private string GenerateFullProperty(PropertyInfo property, string indent)
        {
            var fieldName = GetFieldName(property.Name);
            var sb = new StringBuilder();

            // Field declaration
            sb.AppendLine($"{indent}private {property.Type} {fieldName};");

            // Property declaration
            sb.AppendLine($"{indent}public {property.Type} {property.Name}");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    get => {fieldName};");

            if (property.HasSetter)
            {
                sb.AppendLine($"{indent}    set");
                sb.AppendLine($"{indent}    {{");

                if (_options.GenerateEqualityCheck)
                {
                    sb.AppendLine($"{indent}        if (EqualityComparer<{property.Type}>.Default.Equals({fieldName}, value)) return;");
                }

                sb.AppendLine($"{indent}        {fieldName} = value;");
                sb.AppendLine($"{indent}        {GetNotificationMethodCall(property.Name)};");

                sb.AppendLine($"{indent}    }}");
            }

            sb.AppendLine($"{indent}}}");

            return sb.ToString();
        }

        private string GenerateMvvmToolkitProperty(PropertyInfo property, string indent)
        {
            var fieldName = GetFieldName(property.Name);
            var sb = new StringBuilder();

            // Field declaration with attribute
            sb.AppendLine($"{indent}[ObservableProperty]");

            if (_options.GenerateFieldAttributes && !string.IsNullOrEmpty(_options.FieldAttributes))
            {
                sb.AppendLine($"{indent}{_options.FieldAttributes}");
            }

            sb.AppendLine($"{indent}private {property.Type} {fieldName};");

            return sb.ToString();
        }

        private string GetFieldName(string propertyName)
        {
            switch (_options.FieldNamingStyle)
            {
                case FieldNamingStyle.UnderscoreCamelCase:
                    return $"_{char.ToLower(propertyName[0])}{propertyName.Substring(1)}";
                case FieldNamingStyle.CamelCase:
                    return $"{char.ToLower(propertyName[0])}{propertyName.Substring(1)}";
                case FieldNamingStyle.PrefixedCamelCase:
                    return $"{_options.FieldPrefix}{char.ToLower(propertyName[0])}{propertyName.Substring(1)}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string GetNotificationMethodCall(string propertyName)
        {
            var methodName = "OnPropertyChanged";

            return _options.UseCallerMemberName
                ? $"{methodName}()"
                : $"{methodName}(nameof({propertyName}))";
        }        
    }
}
