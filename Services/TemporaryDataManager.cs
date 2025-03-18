using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using FixedLengthFile_Cleaner.Models;

namespace FixedLengthFile_Cleaner.Services;

/// <summary>
/// Helper methods for managing temporary resource allocation
/// </summary>
public class TemporaryDataManager
{
    //////////
    /// Public
    //////////
    
    public TemporaryDataManager()
    {
        if (Directory.Exists(PathToTemporaryDirectory()))
        {
            Directory.Delete(PathToTemporaryDirectory(), true);
        }
        Directory.CreateDirectory(PathToTemporaryDirectory());
    }
    
    public void CreateTemporaryDirectory()
    {
        if (Directory.Exists(PathToTemporaryDirectory()))
        {
            DeleteTemporaryDirectory();
        }

        // Only the "cleaned" directory needs to be created since when decompressing a ZIP it requires the output
        // folder doesn't already exist.
        Directory.CreateDirectory(PathToCleanedDirectory());
    }

    public void DeleteTemporaryDirectory()
    {
        if (Directory.Exists(PathToTemporaryDirectory()))
        {
            Directory.Delete(PathToTemporaryDirectory(), true);
        }
    }

    public string PathToTemporaryDirectory() => Path.Combine(Path.GetTempPath(), Program.ApplicationName);
    public string PathToCleanedDirectory() => Path.Combine(PathToTemporaryDirectory(), "Cleaned");
}