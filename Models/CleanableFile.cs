using System;
using System.IO;
using System.Threading.Tasks;

namespace FixedLengthFile_Cleaner.Models;

public enum CleanableFileType
{
    TextFile,
    ZipFile,
}

public class CleanableFile
{
    const string DefaultOutputFilenameSuffix = "_cleaned"; 
    
    public string InputFilePath { get; set; }
    public string OutputFilePath { get; set; }
    public CleanableFileType FileType { get; set; }
    public int NumberOfQuotes { get; set; }
    
    public CleanableFile(string inputFilePath, string? outputFilePath = null, CleanableFileType? fileType = null)
    {
        InputFilePath = inputFilePath;
        
        string extension = Path.GetExtension(inputFilePath);
        FileType = fileType 
                   ?? (inputFilePath.EndsWith("zip") 
                       ? CleanableFileType.ZipFile 
                       : CleanableFileType.TextFile);
        
        if (outputFilePath is null)
        {
            // change the output file path so it doesn't overwrite the original
            if (extension == String.Empty)
            {
                OutputFilePath = inputFilePath + DefaultOutputFilenameSuffix;
            }
            else
            {
                // If the file has an extension, insert the suffix before it.
                OutputFilePath = String.Concat(Path.ChangeExtension(inputFilePath, null),
                    DefaultOutputFilenameSuffix, extension);
            }
        }
        else
        {
            OutputFilePath = outputFilePath;
        }
        
    }
    
    
    public static Task Clean(CleanableFile textFile)
    {
        return Task.Run(() =>
        {
            try
            {
                using (StreamReader input = new StreamReader(textFile.InputFilePath))
                using (StreamWriter output = new StreamWriter(textFile.OutputFilePath))
                {
                    int character;
                    while ((character = input.Read()) != -1) // Read character by character
                    {
                        // Replace quotation marks with spaces
                        if (character == '"')
                        {
                            textFile.NumberOfQuotes++;
                            character = ' ';
                        }

                        output.Write((char)character);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        });
    }
}