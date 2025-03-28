using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using FixedLengthFile_Cleaner.Models;
using Serilog;

namespace FixedLengthFile_Cleaner.Services;

/// <summary>
/// Template pattern for handling <see cref="Cleanable"/>.
/// </summary>
public class Cleaner
{
    private StatusMessenger.StatusMessenger _statusMessenger = StatusMessenger.StatusMessenger.GetInstance();

    /// <summary>
    /// Creates a <see cref="Task"/> that encapsulates the business logic for processing a <see cref="Cleanable"/> based 
    /// on its <see cref="Cleanable.Type"/>.
    /// </summary>
    /// <param name="cleanable">The model to process.</param>
    /// <returns>A <see cref="Task"/> object.</returns>
    /// <exception cref="Exception">The <see cref="Cleanable.Type"/> for this <see cref="Cleanable"/> is not
    /// supported</exception>
    public Task Clean(Cleanable cleanable)
    {
        if (cleanable.Type == CleanableType.Folder)
        {
            return CleanFolder(cleanable);
        }

        if (cleanable.Type == CleanableType.TextFile)
        {
            return CleanTextFile(cleanable);
        }

        if (cleanable.Type == CleanableType.ZipFile)
        {
            return CleanZipFile(cleanable);
        }

        throw new Exception("Unknown type");
    }

    private Task CleanFolder(Cleanable folder)
    {
        return Task.Run(async () =>
        {
            var cf = App.FetchService<CleanableFactory>();

            Log.Logger.Information("Creating collection from folder contents...");

            Cleanable[] cleanables;
            if (folder.InputPath == folder.OutputPath)
            {
                cleanables = cf.CreateFromFolder(folder.InputPath, null);
            }
            else
            {
                Directory.CreateDirectory(folder.OutputPath);
                cleanables = cf.CreateFromFolder(folder.InputPath, folder.OutputPath);
            }

            int i = 1;
            foreach (var cleanable in cleanables)
            {
                _statusMessenger.SetStatus($"Cleaning {Path.GetFileName(cleanable.InputPath)}...");
                _statusMessenger.SetProgress($"{i++}/{cleanables.Length}");
                await Clean(cleanable);
                folder.NumberOfQuotes += cleanable.NumberOfQuotes;
            }
        });
    }

    private Task CleanTextFile(Cleanable textFile)
    {
        return Task.Run(() =>
        {
            Log.Logger.Information($"Cleaning file {textFile.InputPath}...");

            // Creating a temporary file handle in case we can't write to the textfile's output path yet
            var tdm = App.FetchService<TemporaryDataManager>();
            using ITemporaryFile tempFile = tdm.CreateTemporaryFile();

            // If the new file is overwriting the old file we must take additional steps
            bool overwritingOriginalFile = textFile.InputPath == textFile.OutputPath;

            string writeToPath;
            if (overwritingOriginalFile)
            {
                // Can't write a file over a file being read, so we'll write it elsewhere and move it once we're done
                writeToPath = tempFile.Path;
            }
            else
            {
                writeToPath = textFile.OutputPath;
            }

            // Todo: Update this method to use the configuration to set the find/replace characters
            // Todo: Refactor to scan over a string rather than a single character
            try
            {
                // Todo: Encapsulate this process to a separate service, let the "number of quotes" be an out parameter
                using (StreamReader input = new StreamReader(textFile.InputPath))
                using (StreamWriter output = new StreamWriter(writeToPath))
                {
                    int character;
                    while ((character = input.Read()) != -1) // Read character by character
                    {
                        // Replace quotation marks with spaces
                        if (character == '"')
                        {
                            character = ' ';
                            textFile.NumberOfQuotes++;
                        }

                        output.Write((char)character);
                    }
                }

                if (overwritingOriginalFile)
                {
                    // Move the output file from the temporary location to its final location, overwriting the input
                    File.Move(writeToPath, textFile.OutputPath, true);
                }

                Log.Logger.Information(
                    $"Successfully cleaned {textFile.InputPath}. Output written to {textFile.OutputPath}. {textFile.NumberOfQuotes} replacements made.");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Error occured while cleaning {textFile.InputPath}:");
            }
        });
    }

    private Task CleanZipFile(Cleanable zipFile)
    {
        return Task.Run(async () =>
        {
            var tdm = App.FetchService<TemporaryDataManager>();

            using (ITemporaryDirectory originalFilesFolder = tdm.CreateTemporaryDirectory())
            using (ITemporaryDirectory cleanedFilesFolder = tdm.CreateTemporaryDirectory())
            {
                Log.Logger.Information($"Creating temporary folder at: {cleanedFilesFolder}");
                cleanedFilesFolder.EnsureExists();

                _statusMessenger.SetStatus("Extracting files...");
                Log.Logger.Information($"Unzipping file to {originalFilesFolder.Path}");

                try
                {
                    ZipFile.ExtractToDirectory(zipFile.InputPath, originalFilesFolder.Path);
                    Log.Logger.Information($"Successfully unpacked {zipFile.InputPath}");
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, $"Error while unpacking {zipFile.InputPath}");
                }

                var cf = App.FetchService<CleanableFactory>();

                Cleanable folder = cf.Create(originalFilesFolder.Path, cleanedFilesFolder.Path);

                await Clean(folder);
                zipFile.NumberOfQuotes += folder.NumberOfQuotes;

                if (File.Exists(zipFile.OutputPath)) File.Delete(zipFile.OutputPath);

                _statusMessenger.SetStatus("Zipping files...");
                Log.Logger.Information($"Zipping files...");

                try
                {
                    ZipFile.CreateFromDirectory(cleanedFilesFolder.Path, zipFile.OutputPath);
                    Log.Logger.Information($"Successfully zipped {originalFilesFolder.Path} to {zipFile.OutputPath}.");
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, $"Error while zipping {cleanedFilesFolder.Path}:");
                }

                _statusMessenger.SetStatus("Deleting temporary files...");

                Log.Logger.Information($"Deleting temporary directories...");
            }
        });
    }
}