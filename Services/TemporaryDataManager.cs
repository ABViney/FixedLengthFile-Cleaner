using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using FixedLengthFile_Cleaner.Models;
using Serilog;

namespace FixedLengthFile_Cleaner.Services;

public interface ITemporaryDirectory : IDisposable
{
    string Path { get; }
    void EnsureExists();
}

/// <summary>
/// Service for managing temporary resources allocation throughout the application's lifetime.
/// </summary>
public class TemporaryDataManager : IDisposable
{
    public string PathToTemporaryDirectory() => Path.Combine(Path.GetTempPath(), Program.ApplicationName);
    
    public TemporaryDataManager()
    {
        Log.Logger.Information("Creating temporary data manager...");
        
        if (Directory.Exists(PathToTemporaryDirectory()))
        {
            Directory.Delete(PathToTemporaryDirectory(), true);
        }
        Directory.CreateDirectory(PathToTemporaryDirectory());
        
        Log.Logger.Information($"Created temporary data directory {PathToTemporaryDirectory()}");
    }

    public ITemporaryDirectory CreateTemporaryDirectory() =>
        new TemporaryDirectory(Path.Combine(PathToTemporaryDirectory(), Path.GetRandomFileName()));
    
    public void Dispose()
    {
        Log.Logger.Information("Deleting temporary data directory");
        
        if (Directory.Exists(PathToTemporaryDirectory()))
        {
            Directory.Delete(PathToTemporaryDirectory(), true);
        }

        if (!Directory.Exists(PathToTemporaryDirectory()))
        {
            Log.Logger.Information($"Deleted temporary data directory {PathToTemporaryDirectory()}");
        }
        else
        {
            Log.Logger.Error($"Failed to delete temporary data directory {PathToTemporaryDirectory()}");
        }
    }

    private class TemporaryDirectory : ITemporaryDirectory
    {
        public string Path { get; }
        
        public void EnsureExists() => Directory.CreateDirectory(Path);

        public TemporaryDirectory(string path)
        {
            Path = path;
        }
        
        public void Dispose()
        {
            Directory.Delete(Path, true);
        }
    }
}

