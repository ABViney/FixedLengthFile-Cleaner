using System;
using System.IO;

namespace FixedLengthFile_Cleaner.Models;

public enum CleanableFileType
{
    TextFile,
    ZipFile,
}

public class CleanableFile
{
    public required string InputFilePath { get; set; }
    public required string OutputFilePath { get; set; }
    public required CleanableFileType FileType { get; set; }
    public int NumberOfQuotes { get; set; }
}