using System;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using FixedLengthFile_Cleaner.Models;

namespace FixedLengthFile_Cleaner.Services;

/// <summary>
/// Factory pattern for creating <see cref="Cleanable"/>.
/// </summary>
public class CleanableFactory
{
    const string DefaultOutputFilenameSuffix = "_cleaned";

    /// <summary>
    /// Create a <see cref="Cleanable"/> from the provided <paramref name="inputFilePath"/>. If no
    /// <paramref name="outputFilePath"/> is specified, the output file defaults to the same location
    /// as the input file, with a default suffix attached.
    /// </summary>
    /// <param name="inputFilePath">The file to clean</param>
    /// <param name="outputFilePath">The location to write the cleaned file</param>
    /// <returns>A model with contextual information.</returns>
    /// <exception cref="NotImplementedException">The input file path specified is a folder and not a file.</exception>
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

    /// <summary>
    /// Generate an array of <see cref="Cleanable"/> for each file in the <paramref name="inputFolderPath"/>. If no
    /// <paramref name="outputFolderPath"/> is specified, the output location for each new <see cref="Cleanable"/> is
    /// as is defined in the <see cref="Create"/> method when no output file path is specified.
    /// </summary>
    /// <param name="inputFolderPath">The folder containing the files to read in.</param>
    /// <param name="outputFolderPath">The output location for each new object.</param>
    /// <returns>An array of <see cref="Cleanable"/>.</returns>
    /// <exception cref="DirectoryNotFoundException">When the <paramref name="inputFolderPath"/> is not a directory.</exception>
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