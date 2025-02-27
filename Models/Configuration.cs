namespace FixedLengthFile_Cleaner.Models;

public class Configuration
{
    public string[] ExcludePatterns { get; set; }

    public override string ToString()
    {
        return $"""
                {nameof(ExcludePatterns)}={string.Join(',', ExcludePatterns)}
                """;
    }
}