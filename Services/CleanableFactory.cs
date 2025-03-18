using System;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using FixedLengthFile_Cleaner.Models;

namespace FixedLengthFile_Cleaner.Services;

public class CleanableFactory
{
    const string DefaultOutputFilenameSuffix = "_cleaned";

    public Cleanable Create(string inputFilePath, string? outputFilePath = null)
    {
        string extension = Path.GetExtension(inputFilePath);

        if (outputFilePath is null)
        {
            // change the output file path so it doesn't overwrite the original
            // If the file has an extension, insert the suffix before it.
            outputFilePath = (extension == String.Empty)
                ? inputFilePath + DefaultOutputFilenameSuffix
                : String.Concat(Path.ChangeExtension(inputFilePath, null),
                    DefaultOutputFilenameSuffix, extension);
        }
        
        
        CleanableType type;
        if (Directory.Exists(inputFilePath))
        {
            throw new NotImplementedException("Cleaning folders is not currently supported.");
        }
        else if (extension == ".zip")
        {
            type = CleanableType.ZipFile;
        }
        else
        {
            type = CleanableType.TextFile;
        }

        Cleanable cleanable = new Cleanable(inputFilePath, outputFilePath, type);
        return cleanable;
    }

    public Cleanable[] CreateFromFolder(string inputFolderPath, string? outputFolderPath)
    {
        if (!Directory.Exists(inputFolderPath)) throw new DirectoryNotFoundException("Input folder doesn't exist.");
        
        string[] inputFolderContents = Directory.GetFileSystemEntries(inputFolderPath);
        bool useDefaultOutputFilenameSuffix = outputFolderPath is null;

        var cleanables = inputFolderContents.Select(inputFilePath =>
        {
            string outputFilePath = useDefaultOutputFilenameSuffix
                ? null
                : Path.Combine(outputFolderPath, Path.GetFileName(inputFilePath));
            Cleanable cleanable = Create(inputFilePath, outputFilePath);
            return cleanable;
        }).ToArray();
        
        return cleanables;
    }
}