using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using FixedLengthFile_Cleaner.Models;

namespace FixedLengthFile_Cleaner.Helpers;

public static class ResourceManagement
{
    private static string PathToTemporaryDirectory() => Path.Combine(Path.GetTempPath(), Program.ApplicationName);
    
    // Config.ini is kept in the same location as the executable.
    public static string PathToConfigurationFile() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
    
    public static void CreateTemporaryDirectory()
    {
        if (Directory.Exists(PathToTemporaryDirectory()))
        {
            DeleteTemporaryDirectory();
        }
        // Only the "cleaned" directory needs to be created since when decompressing a ZIP it requires the output
        // folder doesn't already exist.
        Directory.CreateDirectory(PathToCleanedDirectory());
    }

    public static void DeleteTemporaryDirectory()
    {
        if (Directory.Exists(PathToTemporaryDirectory()))
        {
            Directory.Delete(PathToTemporaryDirectory(), true);
        }
    }

    public static void CompressCleanedFiles(string outputPath)
    {
        if (File.Exists(outputPath)) File.Delete(outputPath);
        ZipFile.CreateFromDirectory(PathToCleanedDirectory(), outputPath);
    }
    
    public static IEnumerable<CleanableFile> DecompressZipFile(string zipFilePath)
    {
        if (!File.Exists(zipFilePath))
        {
            throw new FileNotFoundException("Error: File could not be found.");
        }

        if (Path.GetExtension(zipFilePath) != ".zip")
        {
            throw new InvalidDataException("Error: File is not a zip file.");
        }
        
        if (Directory.Exists(PathToOriginalDirectory())) Directory.Delete(PathToOriginalDirectory(), true);
        ZipFile.ExtractToDirectory(zipFilePath, PathToOriginalDirectory());

        string[] unpackedFiles = Directory.GetFileSystemEntries(PathToOriginalDirectory());
        
        // Todo: This should be encapsulated to a factory pattern
        var config = App.FetchService<Configuration>();
        var cleanableFiles = unpackedFiles.Select(filepath => new CleanableFile(
            filepath, 
            Path.Combine(PathToCleanedDirectory(), Path.GetFileName(filepath)), 
            Path.GetExtension(filepath) != ".zip" ? CleanableFileType.TextFile : CleanableFileType.ZipFile, 
            config.ExcludePatterns.Contains(Path.GetExtension(filepath))));
        return cleanableFiles;
    }

    public static string? ReadConfigurationFile()
    {
        if (!File.Exists(PathToConfigurationFile()))
        {
            return null;
        }
        
        return File.ReadAllText(PathToConfigurationFile());
    }

    public static void WriteConfigurationFile(string configurationText)
    {
        File.WriteAllText(PathToConfigurationFile(), configurationText);
    }
    
    ////////////
    /// Private
    ////////////
    
    private static string PathToCleanedDirectory() => Path.Combine(PathToTemporaryDirectory(), "Cleaned");
    private static string PathToOriginalDirectory() => Path.Combine(PathToTemporaryDirectory(), "Original");
}