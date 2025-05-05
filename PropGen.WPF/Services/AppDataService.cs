using System.IO;
using System.Text.Json;
using PropGen.Core.Models;
using PropGen.WPF.Helpers;

namespace PropGen.WPF.Services
{
    /// <summary>
    /// Provides services for managing and persisting application state data.
    /// </summary>
    public class AppDataService : IAppDataService
    {        
        private readonly string _appFolder;
        private const string AppDataFileName = "AppData.json";
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AppDataService()
        {
#if DEBUG
            _appFolder = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "", "AppData");
#else
            _appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PropGenMVVM");
#endif
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true                
            };
        }

        public void Save(ApplicationData data)
        {
            Directory.CreateDirectory(_appFolder);

            var json = JsonSerializer.Serialize(data, _jsonSerializerOptions);

            File.WriteAllText(Path.Combine(_appFolder, AppDataFileName), json);
        }

        public ApplicationData Load()
        {
            string filePath = Path.Combine(_appFolder, AppDataFileName);

            if (!File.Exists(filePath))
                return new ApplicationData(); // Return defaults if no file yet

            var json = File.ReadAllText(filePath);
            var appData = JsonSerializer.Deserialize<ApplicationData>(json) ?? new ApplicationData();
            // Make sure the saved window can be displayed on the screen
            WindowPlacementHelper.ApplySafeWindowPlacement(appData.Window);
            return appData;
        }
    }

    /// <summary>
    /// Represents the persistent state of the application.
    /// </summary>
    public class ApplicationData
    {
        public bool IsFileParser { get; set; } = false;

        public CodeGenerationOptions Options { get; set; } = new CodeGenerationOptions
        {           
            ImplementNotificationInterface = true,
            FieldNamingStyle = FieldNamingStyle.CamelCase,
            FieldPrefix = string.Empty,
            UseCallerMemberName = false,
            GenerateEqualityCheck = true,
            WrapInRegions = true,
            IsCompactStyle = true,
            IsMvvmToolkitStyle = false            
        };

        public WindowLocationAndSize Window { get; set; } = new WindowLocationAndSize
        {
            Left = 200,
            Top = 200,
            Width = 1200,
            Height = 800
        };
    }  
}