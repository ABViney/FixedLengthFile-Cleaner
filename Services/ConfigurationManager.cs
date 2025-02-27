using System;
using FixedLengthFile_Cleaner.Helpers;
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

    private static ConfigurationManager _instance;


    ///////////
    /// Public 
    ///////////
    public Configuration GetConfiguration() => _configuration;

    public void SetExcludePatterns(string[] excludePatterns)
    {
        _configuration = new Configuration
        {
            ExcludePatterns = excludePatterns,
        };

        SaveConfiguration();
    }


    /////////////
    /// Internal
    /////////////
    private Configuration _configuration;

    private ConfigurationManager()
    {
        ReadConfiguration();
    }

    private string[] DefaultExcludePatterns() => new[] { "*.csv", "*.xlsx", "*.xls" };

    private Configuration CreateDefaultConfiguration()
    {
        Configuration defaultConfig = new Configuration
        {
            ExcludePatterns = DefaultExcludePatterns(),
        };

        return defaultConfig;
    }


    private void ReadConfiguration()
    {
        var configText = ResourceManagement.ReadConfigurationFile();
        if (configText is null)
        {
            _configuration = CreateDefaultConfiguration();
            SaveConfiguration();
        }
        else
        {
            string[] excludePatterns = [];
            string[] fields = configText.Split(';');
            foreach (string field in fields)
            {
                string[] keyvalue = field.Split('=');
                if (keyvalue.Length != 2) continue;
                switch (keyvalue[0])
                {
                    case nameof(Configuration.ExcludePatterns):
                        excludePatterns = keyvalue[1].Split(',');
                        break;
                }
            }

            _configuration = new Configuration
            {
                ExcludePatterns = excludePatterns,
            };
        }

        Console.WriteLine($"Config loaded:\r\n" + _configuration.ToString());
    }

    private void SaveConfiguration()
    {
        ResourceManagement.WriteConfigurationFile(_configuration.ToString());
    }
}