using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using FixedLengthFile_Cleaner.Models;

namespace FixedLengthFile_Cleaner.Services;

/// <summary>
/// Template pattern for handling <see cref="Cleanable"/>.
/// </summary>
public class Cleaner
{
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

            foreach (var cleanable in cleanables)
            {
                await Clean(cleanable);
                folder.NumberOfQuotes += cleanable.NumberOfQuotes;
            }
        });
    }

    private Task CleanTextFile(Cleanable textFile)
    {
        return Task.Run(() =>
        {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
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

            Directory.CreateDirectory(cleanedFilesFolder);

            ZipFile.ExtractToDirectory(zipFile.InputPath, originalFilesFolder);

            var cf = App.FetchService<CleanableFactory>();

            Cleanable[] cleanables = cf.CreateFromFolder(originalFilesFolder, cleanedFilesFolder);

            foreach (var cleanable in cleanables)
            {
                await Clean(cleanable);
                zipFile.NumberOfQuotes += cleanable.NumberOfQuotes;
            }

            if (File.Exists(zipFile.OutputPath)) File.Delete(zipFile.OutputPath);
            ZipFile.CreateFromDirectory(cleanedFilesFolder, zipFile.OutputPath);
            Directory.Delete(originalFilesFolder, true);
            Directory.Delete(cleanedFilesFolder, true);
        });
    }
}