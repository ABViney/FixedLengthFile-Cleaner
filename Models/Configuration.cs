namespace FixedLengthFile_Cleaner.Models;

public class Configuration
{
    public const string APPLICATION_NAME = "FixedLengthFile_Cleaner";
    
    public string PathToTemporaryDirectory { get; set; }
    public string[] ExcludePatterns { get; set; }
}