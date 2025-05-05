using System.Reflection;

namespace PropGen.WPF.Helpers
{
    public static class AppVersionHelper
    {
        public static string AssemblyVersion =>
            Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";

        public static string FileVersion =>
            Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyFileVersionAttribute>()?
                .Version ?? "Unknown";

        public static string InformationalVersion
        {
            get
            {
                var raw = Assembly.GetEntryAssembly()?
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion ?? "Unknown";

                // Strip anything after '+' (e.g., "+commitsha")
                var plusIndex = raw.IndexOf('+');
                return plusIndex >= 0 ? raw.Substring(0, plusIndex) : raw;
            }
        }
    }
}
