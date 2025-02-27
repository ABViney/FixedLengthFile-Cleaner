using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FixedLengthFile_Cleaner.Helpers;
using FixedLengthFile_Cleaner.Models;
using FixedLengthFile_Cleaner.Services;
using Microsoft.Extensions.DependencyInjection;


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
            desktop.MainWindow = new MainWindow();
            
            ////////////
            // Services
            ////////////
            {
                var services = new ServiceCollection();

                services.AddSingleton<Configuration>(x => ConfigurationManager.GetInstance().GetConfiguration());
                
                Services = services.BuildServiceProvider();
            }
            
            //////////////////
            // Event handlers
            //////////////////
            desktop.Startup += (sender, args) => _DeleteTemporaryResources();
            desktop.Exit += (sender, args) => _DeleteTemporaryResources();
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Clean up temporary directories used during runtime.
    /// </summary>
    private void _DeleteTemporaryResources()
    {
        ResourceManagement.DeleteTemporaryDirectory();
    }
}