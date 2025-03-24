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

            // Todo: Update this method to use the configuration to set the find/replace characters
            // Todo: Refactor to scan over a string rather than a single character
            try
            {
                using (StreamReader input = new StreamReader(textFile.InputPath))
                using (StreamWriter output = new StreamWriter(textFile.OutputPath))
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
            string originalFilesFolder = Path.Combine(tdm.PathToTemporaryDirectory(), Path.GetRandomFileName());
            string cleanedFilesFolder = originalFilesFolder + "_cleaned";

            Log.Logger.Information($"Creating temporary folder at: {cleanedFilesFolder}");

            Directory.CreateDirectory(cleanedFilesFolder);

            _statusMessenger.SetStatus("Extracting files...");
            Log.Logger.Information($"Unzipping file to {originalFilesFolder}");

            try
            {
                ZipFile.ExtractToDirectory(zipFile.InputPath, originalFilesFolder);
                Log.Logger.Information($"Successfully unpacked {zipFile.InputPath}");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Error while unpacking {zipFile.InputPath}");
            }

            var cf = App.FetchService<CleanableFactory>();

            Cleanable folder = cf.Create(originalFilesFolder, cleanedFilesFolder);

            await Clean(folder);
            zipFile.NumberOfQuotes += folder.NumberOfQuotes;

            if (File.Exists(zipFile.OutputPath)) File.Delete(zipFile.OutputPath);

            _statusMessenger.SetStatus("Zipping files...");
            Log.Logger.Information($"Zipping files...");

            try
            {
                ZipFile.CreateFromDirectory(cleanedFilesFolder, zipFile.OutputPath);
                Log.Logger.Information($"Successfully zipped {originalFilesFolder} to {zipFile.OutputPath}.");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Error while zipping {cleanedFilesFolder}:");
            }

            _statusMessenger.SetStatus("Deleting temporary files...");

            try
            {
                Log.Logger.Information($"Deleting temporary directories...");
                Directory.Delete(originalFilesFolder, true);
                Directory.Delete(cleanedFilesFolder, true);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Error occured while deleting temporary directories:");
            }
        });
    }
}