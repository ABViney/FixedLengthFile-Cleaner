using System;
using System.Runtime.InteropServices.JavaScript;
using Serilog;

namespace FixedLengthFile_Cleaner.Models;

public class Configuration
{
    public string Find { get; set; }
    public string Replace { get; set; }
    public string[] ExcludePatterns { get; set; }
    public string OutputSuffix { get; set; }
}