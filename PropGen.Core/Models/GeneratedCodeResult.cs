using System.Collections.Generic;

namespace PropGen.Core.Models
{
    public class GeneratedCodeResult
    {     
        public string Content { get; set; }        
        public List<ClassInfo> Classes { get; set; } 
        public int PropertyCount { get; set; }
    }
}
