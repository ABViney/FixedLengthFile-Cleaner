using System;
using System.IO;
using System.Threading.Tasks;

namespace FixedLengthFile_Cleaner.Models;

/// <summary>
/// Supported filesystem types for <see cref="Cleanable"/>.
/// </summary>
public enum CleanableType
{
    TextFile,
    ZipFile,
}

/// <summary>
/// A model of a filesystem resource designated for cleaning.
/// </summary>
public class Cleanable
{
    
    public string InputFilePath { get; }
    public string OutputFilePath { get; set; }
    public CleanableType Type { get; }
    public int NumberOfQuotes { get; set; }
    
    public Cleanable(string inputFilePath, string outputFilePath, CleanableType type)
    {
        InputFilePath = inputFilePath;
        OutputFilePath = outputFilePath;
        Type = type;
    }
}