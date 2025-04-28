using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace PropGen.Core.Models
{
    public class PropertyInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool HasSetter { get; set; }
        public List<string> Attributes { get; set; }
        public Location Location { get; set; }
        public int LineNumber { get; set; }
    }
}
