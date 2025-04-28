using System.Collections.Generic;
using System.ComponentModel;

namespace PropGen.Core.Models
{
    public class CodeGenerationOptions
    {        
        public string NotificationInterfaceName { get; set; } = "INotifyPropertyChanged";
        public bool ImplementNotificationInterface { get; set; } = true;    
        public FieldNamingStyle FieldNamingStyle { get; set; } = FieldNamingStyle.UnderscoreCamelCase;
        public string FieldPrefix { get; set; } = string.Empty;
        public bool UseCallerMemberName { get; set; } = false;
        public bool GenerateEqualityCheck { get; set; } = true;        
        public bool WrapInRegions { get; set; } = false;
        public bool IsCompactStyle { get; set; } = false;
        public bool IsMvvmToolkitStyle { get; set; } = false;
        //public bool MakeClassPartial { get; set; } = false;
        public bool GenerateFieldAttributes { get; set; } = true;
        public string FieldAttributes { get; set; } = string.Empty;
    }

    public enum FieldNamingStyle
    {        
        UnderscoreCamelCase,     
        CamelCase,        
        PrefixedCamelCase
    }        
}
