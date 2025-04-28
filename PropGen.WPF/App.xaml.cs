using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Windows;
using PropGen.Core.Services;
using PropGen.WPF.Views;
using PropGen.WPF.ViewModels;
using PropGen.WPF.Services;

namespace PropGen.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Initialize your application here

            var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // Core services
                services.AddSingleton<ITextParserService, TextParserService>();
                services.AddSingleton<ISourceParserService, SourceParserService>();                
                services.AddSingleton<IPropertyCodeGenerator, PropertyCodeGenerator>();

                // WPF
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();

                // Services
                services.AddTransient<IDialogService, DialogService>();
                services.AddSingleton<IAppDataService, AppDataService>();
            })
            .Build();

            host.Services.GetRequiredService<MainWindow>().Show();
        }      
    }
}
