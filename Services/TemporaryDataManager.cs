using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using FixedLengthFile_Cleaner.Models;

namespace FixedLengthFile_Cleaner.Services;

/// <summary>
/// Service for managing temporary resources allocation throughout the application's lifetime.
/// </summary>
public class TemporaryDataManager : IDisposable
{
    public TemporaryDataManager()
    {
        if (Directory.Exists(PathToTemporaryDirectory()))
        {
            Directory.Delete(PathToTemporaryDirectory(), true);
        }
        Directory.CreateDirectory(PathToTemporaryDirectory());
    }

    public void Dispose()
    {
        if (Directory.Exists(PathToTemporaryDirectory()))
        {
            Directory.Delete(PathToTemporaryDirectory(), true);
        }
    }

    public string PathToTemporaryDirectory() => Path.Combine(Path.GetTempPath(), Program.ApplicationName);
}