using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using FixedLengthFile_Cleaner.Models;

namespace FixedLengthFile_Cleaner.Services;

public class Cleaner
{
    public Task Clean(Cleanable cleanable)
    {
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
        throw new NotImplementedException("CleanFolder is not implemented");
    }
    
    private Task CleanTextFile(Cleanable textFile)
    {
        return Task.Run(() =>
        {
            // Todo: Update this method to use the configuration to set the find/replace characters
            // Todo: Refactor to scan over a string rather than a single character
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
            
            ZipFile.ExtractToDirectory(zipFile.InputFilePath, originalFilesFolder);;

            var cf = App.FetchService<CleanableFactory>();

            Cleanable[] cleanables = cf.CreateFromFolder(originalFilesFolder, cleanedFilesFolder);

            foreach (var cleanable in cleanables)
            {
                await Clean(cleanable);
                zipFile.NumberOfQuotes += cleanable.NumberOfQuotes;
            }
            
            if (File.Exists(zipFile.OutputFilePath)) File.Delete(zipFile.OutputFilePath);
            ZipFile.CreateFromDirectory(cleanedFilesFolder, zipFile.OutputFilePath);
            Directory.Delete(originalFilesFolder, true);
            Directory.Delete(cleanedFilesFolder, true);
        });
    }
}