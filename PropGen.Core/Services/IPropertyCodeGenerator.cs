using PropGen.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropGen.Core.Services
{
    public interface IPropertyCodeGenerator
    {
        GeneratedCodeResult GenerateProperties(IEnumerable<ClassInfo> classes, CodeGenerationOptions options);
    }
}
