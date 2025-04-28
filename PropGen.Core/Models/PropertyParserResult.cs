using System.Collections.Generic;

namespace PropGen.Core.Models
{
    public class PropertyParserResult
    {
        public List<ClassInfo> Classes { get; } = new List<ClassInfo>();
        public List<ProcessingIssue> Issues { get; } = new List<ProcessingIssue>();
        public bool HasIssues => Issues.Count > 0;
    }
}
