using Microsoft.CodeAnalysis;

namespace PropGen.Core.Models
{
    public class ProcessingIssue
    {
        public IssueSeverity Severity { get; set; }
        public string Message { get; set; }
        public Location Location { get; set; }
        public string CodeElement { get; set; }
        public int LineNumber { get; set; }
    }

    public enum IssueSeverity
    {
        Warning,
        Error
    }
}
