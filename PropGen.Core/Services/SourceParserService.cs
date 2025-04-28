using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropGen.Core.Models;
using System;


namespace PropGen.Core.Services
{
    public class SourceParserService : ISourceParserService
    {
        public PropertyParserResult ParseFile(string filePath)
        {
            var result = new PropertyParserResult();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                result.Issues.Add(new ProcessingIssue
                {
                    Severity = IssueSeverity.Error,
                    Message = "File path cannot be null or empty"
                });
                return result;
            }

            if (!File.Exists(filePath))
            {
                result.Issues.Add(new ProcessingIssue
                {
                    Severity = IssueSeverity.Error,
                    Message = $"Source file not found: {filePath}"
                });
                return result;
            }

            try
            {
                var code = File.ReadAllText(filePath);
                return ParseSource(code);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                result.Issues.Add(new ProcessingIssue
                {
                    Severity = IssueSeverity.Error,
                    Message = $"Error reading file {filePath}: {ex.Message}"
                });
                return result;
            }
        }

        public PropertyParserResult ParseSource(string sourceCode)
        {
            var result = new PropertyParserResult();

            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                result.Issues.Add(new ProcessingIssue
                {
                    Severity = IssueSeverity.Error,
                    Message = "Source code cannot be null or empty"
                });
                return result;
            }

            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            }
            catch (Exception ex)
            {
                result.Issues.Add(new ProcessingIssue
                {
                    Severity = IssueSeverity.Error,
                    Message = $"Failed to parse source code: {ex.Message}"
                });
                return result;
            }

            var diagnostics = syntaxTree.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();

            if (diagnostics.Any())
            {
                result.Issues.AddRange(diagnostics.Select(d => new ProcessingIssue
                {
                    Severity = IssueSeverity.Error,
                    Message = d.GetMessage(),
                    Location = d.Location
                }));
                return result;
            }

            var root = syntaxTree.GetRoot();
            FindCandidateClasses(root, result);
            return result;
        }

        private void FindCandidateClasses(SyntaxNode root, PropertyParserResult result)
        {
            var classDeclarations = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .ToList();

            foreach (var classDecl in classDeclarations)
            {
                try
                {
                    if (ShouldImplementINotifyPropertyChanged(classDecl, result))
                    {
                        var classInfo = new ClassInfo
                        {
                            ClassName = classDecl.Identifier.Text,
                            Properties = FindNotifiableProperties(classDecl, result),
                            Namespace = GetNamespace(classDecl, result),
                            Location = classDecl.GetLocation()
                        };

                        result.Classes.Add(classInfo);
                    }
                }
                catch (Exception ex)
                {
                    result.Issues.Add(new ProcessingIssue
                    {
                        Severity = IssueSeverity.Warning,
                        Message = $"Failed to process class {classDecl.Identifier.Text}: {ex.Message}",
                        Location = classDecl.GetLocation(),
                        CodeElement = classDecl.Identifier.Text
                    });
                }
            }
        }

        private bool ShouldImplementINotifyPropertyChanged(ClassDeclarationSyntax classDecl, PropertyParserResult result)
        {
            try
            {
                if (classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                {
                    return false;
                }

                var alreadyImplements = classDecl.BaseList?.Types
                    .Any(t => t.ToString() == "INotifyPropertyChanged") ?? false;

                if (alreadyImplements) return false;

                var hasAttribute = classDecl.AttributeLists
                    .SelectMany(a => a.Attributes)
                    .Any(a => a.Name.ToString() == "Observable");

                var hasNotifiableProperties = classDecl.Members
                    .OfType<PropertyDeclarationSyntax>()
                    .Any(p => IsNotifiableProperty(p, result));

                return hasAttribute || hasNotifiableProperties;
            }
            catch (Exception ex)
            {
                result.Issues.Add(new ProcessingIssue
                {
                    Severity = IssueSeverity.Warning,
                    Message = $"Error checking INPC implementation for class: {ex.Message}",
                    Location = classDecl.GetLocation(),
                    CodeElement = classDecl.Identifier.Text
                });
                return false;
            }
        }

        private List<PropertyInfo> FindNotifiableProperties(ClassDeclarationSyntax classDecl, PropertyParserResult result)
        {
            var properties = new List<PropertyInfo>();

            foreach (var member in classDecl.Members.OfType<PropertyDeclarationSyntax>())
            {
                try
                {
                    if (IsNotifiableProperty(member, result))
                    {
                        properties.Add(new PropertyInfo
                        {
                            Name = member.Identifier.Text,
                            Type = member.Type.ToString(),
                            HasSetter = member.AccessorList?.Accessors
                                .Any(a => a.Kind() == SyntaxKind.SetAccessorDeclaration) ?? false,
                            Location = member.GetLocation()
                        });
                    }
                }
                catch (Exception ex)
                {
                    result.Issues.Add(new ProcessingIssue
                    {
                        Severity = IssueSeverity.Warning,
                        Message = $"Failed to process property {member.Identifier.Text}: {ex.Message}",
                        Location = member.GetLocation(),
                        CodeElement = member.Identifier.Text
                    });
                }
            }

            return properties;
        }

        private bool IsNotifiableProperty(PropertyDeclarationSyntax property, PropertyParserResult result)
        {
            try
            {
                if (property.ExplicitInterfaceSpecifier != null)
                    return false;

                if (property.AccessorList?.Accessors.Any(a => a.Body != null) ?? false)
                    return false;

                if (property.AttributeLists.Count > 0)
                    return false;

                return property.AccessorList?.Accessors
                    .Any(a => a.Kind() == SyntaxKind.SetAccessorDeclaration) ?? false;
            }
            catch (Exception ex)
            {
                result.Issues.Add(new ProcessingIssue
                {
                    Severity = IssueSeverity.Warning,
                    Message = $"Error checking property {property.Identifier.Text}: {ex.Message}",
                    Location = property.GetLocation(),
                    CodeElement = property.Identifier.Text
                });
                return false;
            }
        }

        private string GetNamespace(ClassDeclarationSyntax classDecl, PropertyParserResult result)
        {
            try
            {
                return classDecl.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>()?.Name.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                result.Issues.Add(new ProcessingIssue
                {
                    Severity = IssueSeverity.Warning,
                    Message = $"Error getting namespace: {ex.Message}",
                    Location = classDecl.GetLocation(),
                    CodeElement = classDecl.Identifier.Text
                });
                return string.Empty;
            }
        }
    }
}
