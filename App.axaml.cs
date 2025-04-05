using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FixedLengthFile_Cleaner.Models;
using FixedLengthFile_Cleaner.Services;
using FixedLengthFile_Cleaner.Services.StatusMessenger;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


namespace FixedLengthFile_Cleaner;

public partial class App : Application
{
    ///////////
    /// Static
    ///////////

    public static IServiceProvider Services { get; private set; }

    public static T FetchService<T>()
    {
        var serviceProvider = Services ??
                              throw new NullReferenceException(
                                  $"Method invoked before property {nameof(Services)} was initialized.");
        return serviceProvider.GetService<T>() ??
               throw new NullReferenceException($"{typeof(T)} service not registered.");
    }

    ///////////
    /// Public
    ///////////
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ////////////
            // Services
            ////////////
            {
                Log.Logger.Information("Setting up services");
                
                var services = new ServiceCollection();
                
                services.AddSingleton<CleanableFactory>();
                services.AddSingleton<Cleaner>();
                services.AddSingleton<ConfigurationManager>(x => ConfigurationManager.GetInstance());
                services.AddSingleton<Configuration>(x => ConfigurationManager.GetInstance().GetConfiguration());
                services.AddSingleton<StatusMessenger>(x => StatusMessenger.GetInstance());
                services.AddSingleton<TemporaryDataManager>();

                Services = services.BuildServiceProvider();
                
                Log.Logger.Information("Services configured");
            }

            desktop.MainWindow = new MainWindow();
            
            //////////////////
            // Event handlers
            //////////////////
            desktop.Exit += (sender, args) => FetchService<TemporaryDataManager>().Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}