using Avalonia;
using System;
using System.IO;
using Serilog;

namespace FixedLengthFile_Cleaner;

class Program
{
    public const string ApplicationName = "FixedLengthFile_Cleaner";
    public const string AppVersion = "0.4.0";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        string pathToLogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{ApplicationName}.log");
        if (File.Exists(pathToLogFile))
        {
            File.Delete(pathToLogFile);
        }

        // Set up Logging
        using var log = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(pathToLogFile)
            .CreateLogger();

        Log.Logger = log;

        Log.Logger.Information("Application started");
        Log.Logger.Information("Version {AppVersion}", AppVersion);

        bool fatalErrorOccured = false;

        try
        {
            // Program lifetime
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Logger.Fatal(ex, "An unexpected error occured causing the requiring the application to terminate.");
            fatalErrorOccured = true;
        }

        Log.Logger.Information("Application stopped");

        if (fatalErrorOccured)
        {
            // Copy log to a unique location so it doesn't get overwritten if the user restarts the app right away.
            string pathToFatalLogFile =
                pathToLogFile + $"(CrashLog_{DateTime.Now.ToString("yyyyMMddHHmmss")})";
            File.Copy(pathToLogFile, pathToFatalLogFile);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}