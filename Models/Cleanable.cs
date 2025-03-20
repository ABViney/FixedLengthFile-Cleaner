using System;
using System.IO;
using System.Threading.Tasks;

namespace FixedLengthFile_Cleaner.Models;

/// <summary>
/// Supported filesystem types for <see cref="Cleanable"/>.
/// </summary>
public enum CleanableType
{
    Folder,
    TextFile,
    ZipFile,
}

/// <summary>
/// A model of a filesystem resource designated for cleaning.
/// </summary>
public class Cleanable
{
    
    public string InputPath { get; }
    public string OutputPath { get; set; }
    public CleanableType Type { get; }
    public int NumberOfQuotes { get; set; }
    
    public Cleanable(string inputPath, string outputPath, CleanableType type)
    {
        InputPath = inputPath;
        OutputPath = outputPath;
        Type = type;
    }
}