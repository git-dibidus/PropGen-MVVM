using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace PropGen.Core.Models
{
    public class ClassInfo
    {
        public string ClassName { get; set; }
        public string Namespace { get; set; }
        public List<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();
        public Location Location { get; set; }
    }
}
