using System;
using System.IO;
using FixedLengthFile_Cleaner.Models;

namespace FixedLengthFile_Cleaner.Services;

public class ConfigurationManager
{
    ///////////
    /// Static
    ///////////
    public static ConfigurationManager GetInstance()
    {
        if (_instance is null)
        {
            _instance = new ConfigurationManager();
        }
        return _instance;
    }
    private const string ConfigurationFileName = "config.ini";
    private static ConfigurationManager _instance;
    
    
    ///////////
    /// Public 
    ///////////
    public Configuration GetConfiguration() => _configuration;

    
    /////////////
    /// Internal
    /////////////
    private Configuration _configuration;
    
    private ConfigurationManager()
    {
        ReadConfiguration();
    }
    
    private Configuration CreateDefaultConfiguration()
    {
        Configuration defaultConfig = new Configuration
        {
            PathToTemporaryDirectory = Path.Combine(Path.GetTempPath(), Program.ApplicationName),
            ExcludePatterns = new string[] { "*.csv", "*.xlsx", "*.xls" },
        };
        
        return defaultConfig;
    }

    private void ReadConfiguration()
    {
        string configurationFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationFileName);
        
        if (File.Exists(configurationFilePath))
        {
            Console.WriteLine("File exists (but I'm not reading it yet");
        }

        _configuration = CreateDefaultConfiguration();
    }
    
    private void SaveConfiguration()
    {
        // Write the configuration to the same location as the executable.
        Console.WriteLine($"Current app domain = {AppDomain.CurrentDomain.BaseDirectory}");
    }
}