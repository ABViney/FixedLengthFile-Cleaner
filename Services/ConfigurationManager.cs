using System;
using System.IO;
using System.Text.Json;
using FixedLengthFile_Cleaner.Models;
using Serilog;

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

    // Config is kept in the same location as the executable.
    public static string PathToConfigurationFile() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    public void UpdateConfiguration(Configuration config)
    {
        _configuration = config;
        SaveConfiguration();
    }

    private void SaveConfiguration()
    {
        if (_configuration is null)
            throw new NullReferenceException($"Cannot save config: {nameof(_configuration)} is null");
        Log.Logger.Information("Saving configuration file.");
        try
        {
            string json = JsonSerializer.Serialize(_configuration, new JsonSerializerOptions { WriteIndented = true });
            WriteConfigurationFile(json);
            Log.Logger.Information("Configuration saved.");
        }
        catch (Exception ex)
        {
            Log.Logger.Warning(ex, "Failed to save configuration file.");
        }
    }

    /////////////
    /// Private
    /////////////
    private Configuration _configuration;

    private ConfigurationManager()
    {
        InstantiateConfiguration();
    }

    private void InstantiateConfiguration()
    {
        var configText = TryReadConfigurationFile();
        if (configText is null)
        {
            Log.Logger.Information("Creating default configuration.");
            _configuration = CreateDefaultConfiguration();
            SaveConfiguration();
        }
        else
        {
            // _configuration = Configuration.FromText(configText);
            try
            {
                Log.Logger.Information("Deserializing configuration...");
                _configuration = JsonSerializer.Deserialize<Configuration>(configText);
                if (_configuration is null) throw new NullReferenceException($"Cannot deserialize configuration:\n {configText}");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Failed to deserialize configuration. Creating default configuration.");
                _configuration = CreateDefaultConfiguration();
            }
        }

        Log.Logger.Information("Config loaded.");
    }

    private string? TryReadConfigurationFile()
    {
        Log.Logger.Information("Attempting to read configuration file.");

        if (!File.Exists(PathToConfigurationFile()))
        {
            Log.Logger.Warning("Could not find configuration file.");
            return null;
        }

        Log.Logger.Information("Found configuration file.");
        return File.ReadAllText(PathToConfigurationFile());
    }

    private void WriteConfigurationFile(string configurationText)
    {
        File.WriteAllText(PathToConfigurationFile(), configurationText);
    }

    private Configuration CreateDefaultConfiguration()
    {
        return new Configuration()
        {
            Find = "\"",
            Replace = " ",
            ExcludePatterns = ["*.csv", "*.xlsx", "*.xls"],
            OutputSuffix = "_cleaned"
        };
    }
}