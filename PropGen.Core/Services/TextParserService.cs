using System;
using System.Collections.Generic;
using System.Linq;
using PropGen.Core.Models;

namespace PropGen.Core.Services
{
    /// <summary>
    /// Service for parsing property definitions from plain text input. Supports two formats:
    /// "Type Name" or "Name,Type" per line. Validates syntax, checks for duplicates, and 
    /// converts valid entries into PropertyInfo objects. Maintains line numbers for error 
    /// reporting and collects all parsing issues while continuing to process subsequent lines.
    /// </summary>
    public class TextParserService : ITextParserService
    {                
        public PropertyParserResult ParseText(string inputText)
        {
            var result = new PropertyParserResult();
            result.Classes.Add(new ClassInfo { ClassName = "", Namespace = "" });

            var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(inputText))
                return result;            

            var lines = inputText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    // Remove trailing semicolon if present
                    line = line.TrimEnd(';');

                    PropertyInfo property;
                    if (line.Contains(','))
                    {
                        // Format: Name, Type
                        var parts = line.Split(',')
                                        .Select(p => p.Trim())
                                        .Where(p => !string.IsNullOrEmpty(p))
                                        .ToArray();

                        if (parts.Length != 2)
                        {
                            result.Issues.Add(NewProcessingIssue(i + 1, $"Line must contain exactly one comma separating name and type. [Input: '{line}']", IssueSeverity.Error));
                            continue;
                        }

                        property = NewPropertyInfo(name: parts[0], type: parts[1], lineNumber: i + 1);                        
                    }
                    else
                    {
                        // Format: Type Name
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length != 2)
                        {
                            result.Issues.Add(NewProcessingIssue(i + 1, $"Line must contain exactly one space separating name and type. [Input: '{line}']", IssueSeverity.Error));
                            continue;
                        }

                        property = NewPropertyInfo(name: parts[1], type: parts[0], lineNumber: i + 1);                        
                    }

                    // Check for duplicate property names
                    if (existingNames.Contains(property.Name))
                    {
                        result.Issues.Add(NewProcessingIssue(i + 1, $"Duplicate property name '{property.Name}'.", IssueSeverity.Warning));
                        continue;
                    }

                    if (result.Issues.Any(x => x.LineNumber == i + 1))
                    {
                        // Skip adding this property if there are errors on the same line
                        continue;
                    }

                    existingNames.Add(property.Name);
                    result.Classes[0].Properties.Add(property);
                }
                catch (Exception ex)
                {
                    result.Issues.Add(NewProcessingIssue(i + 1, $"{ex.Message} [Input: '{line}']", IssueSeverity.Error));
                }
            }

            return result;
        }

        private PropertyInfo NewPropertyInfo(string name, string type, int lineNumber)
        {
            return new PropertyInfo
            {
                Name = name,
                Type = type,
                HasSetter = true,
                LineNumber = lineNumber
            };
        }

        private ProcessingIssue NewProcessingIssue(int lineNumber, string message, IssueSeverity severity)
        {
            return new ProcessingIssue
            {
                Severity = severity,
                Message = message,
                LineNumber = lineNumber
            };
        }
    }
}
