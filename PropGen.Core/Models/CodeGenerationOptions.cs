using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PropGen.Core.Models
{
    //public class CodeGenerationOptions
    //{
    //    public string NotificationInterfaceName { get; set; }
    //    public bool ImplementNotificationInterface { get; set; }
    //    public FieldNamingStyle FieldNamingStyle { get; set; }
    //    public string FieldPrefix { get; set; }
    //    public bool UseCallerMemberName { get; set; }
    //    public bool GenerateEqualityCheck { get; set; }
    //    public bool WrapInRegions { get; set; }
    //    public bool IsCompactStyle { get; set; }
    //    public bool IsMvvmToolkitStyle { get; set; }
    //}

    public class CodeGenerationOptions : INotifyPropertyChanged
    {
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Property NotificationInterfaceName
        private string _notificationInterfaceName;
        public string NotificationInterfaceName
        {
            get => _notificationInterfaceName;
            set => SetProperty(ref _notificationInterfaceName, value);
        }
        #endregion

        #region Property ImplementNotificationInterface
        private bool _implementNotificationInterface;
        public bool ImplementNotificationInterface
        {
            get => _implementNotificationInterface;
            set => SetProperty(ref _implementNotificationInterface, value);
        }
        #endregion

        #region Property FieldNamingStyle
        private FieldNamingStyle _fieldNamingStyle;
        public FieldNamingStyle FieldNamingStyle
        {
            get => _fieldNamingStyle;
            set => SetProperty(ref _fieldNamingStyle, value);
        }
        #endregion

        #region Property FieldPrefix
        private string _fieldPrefix;
        public string FieldPrefix
        {
            get => _fieldPrefix;
            set => SetProperty(ref _fieldPrefix, value);
        }
        #endregion

        #region Property UseCallerMemberName
        private bool _useCallerMemberName;
        public bool UseCallerMemberName
        {
            get => _useCallerMemberName;
            set => SetProperty(ref _useCallerMemberName, value);
        }
        #endregion

        #region Property GenerateEqualityCheck
        private bool _generateEqualityCheck;
        public bool GenerateEqualityCheck
        {
            get => _generateEqualityCheck;
            set => SetProperty(ref _generateEqualityCheck, value);
        }
        #endregion

        #region Property WrapInRegions
        private bool _wrapInRegions;
        public bool WrapInRegions
        {
            get => _wrapInRegions;
            set => SetProperty(ref _wrapInRegions, value);
        }
        #endregion

        #region Property IsCompactStyle
        private bool _isCompactStyle;
        public bool IsCompactStyle
        {
            get => _isCompactStyle;
            set => SetProperty(ref _isCompactStyle, value);
        }
        #endregion

        #region Property IsMvvmToolkitStyle
        private bool _isMvvmToolkitStyle;
        public bool IsMvvmToolkitStyle
        {
            get => _isMvvmToolkitStyle;
            set => SetProperty(ref _isMvvmToolkitStyle, value);
        }
        #endregion
    }

    public enum FieldNamingStyle
    {        
        UnderscoreCamelCase,     
        CamelCase,        
        PrefixedCamelCase
    }        
}
