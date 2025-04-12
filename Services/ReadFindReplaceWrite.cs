using System;
using System.IO;

namespace FixedLengthFile_Cleaner.Services;

public class ReadFindReplaceWrite
{
    public static int Process(string find, string replace, string inputFilePath, string outputFilePath)
    {
        if (String.IsNullOrEmpty(find))
        {
            throw new ArgumentException("Find string must not be empty.");
        }

        if (find.Length == 1)
        {
            switch (replace.Length)
            {
                case 0:
                    return FindCharDelete(find[0], inputFilePath, outputFilePath);
                case 1:
                    return FindCharReplaceChar(find[0], replace[0], inputFilePath, outputFilePath);
                default:
                    return FindCharReplaceString(find[0], replace, inputFilePath, outputFilePath);
            }
        }

        if (find.Length > 1)
        {
            switch (replace.Length)
            {
                case 0:
                    return FindStringDelete(find, inputFilePath, outputFilePath);
                case 1:
                    return FindStringReplaceChar(find, replace[0], inputFilePath, outputFilePath);
                default:
                    return FindStringReplaceString(find, replace, inputFilePath, outputFilePath);
            }
        }
        
        throw new Exception("This section is unreachable.");
    }

    public static int FindCharDelete(char find, string inputFilePath, string outputFilePath)
    {
        int replacementsMade = 0;
        using (StreamReader input = new StreamReader(inputFilePath))
        using (StreamWriter output = new StreamWriter(outputFilePath))
        {
            int character;
            while ((character = input.Read()) != -1) // Read character by character
            {
                // Replace quotation marks with spaces
                if (character == find)
                {
                    replacementsMade++;
                }
                else
                {
                    output.Write((char)character);
                }
            }
        }
        return replacementsMade;
    }

    public static int FindStringDelete(string find, string inputFilePath, string outputFilePath)
    {
        int[] buffer = new int[find.Length]; // This is the window for storing characters being compared to the find string
        int index = 0; // Where in the window is currently being written to
        int replacementsMade = 0;

        using (StreamReader input = new StreamReader(inputFilePath))
        using (StreamWriter output = new StreamWriter(outputFilePath))
        {
            while ((buffer[index] = input.Read()) != -1) // Read character by character
            {
                if (buffer[index] == find[index]) // Checking current character for a match
                {
                    index++;
                    if (index == find.Length) // If the index reaches the end of the find string, we have a match.
                    {
                        index = 0;
                        replacementsMade++;
                    }
                }
                else if (index > 0) // Earlier matches were a false positive. Write what was written to the buffer
                {
                    for (int i = 0; i < index; i++)
                    {
                        output.Write((char)buffer[i]);
                    }

                    index = 0;
                }
                else
                {
                    output.Write((char)buffer[index]);
                }
            }

            if (index > 0) // Reached end of file while attempting a match. Flush buffer
            {
                for (int i = 0; i < index; i++)
                {
                    output.Write((char)buffer[i]);
                }
            }
        }
        return replacementsMade;
    }
    
    public static int FindCharReplaceChar(char find, char replace, string inputFilePath, string outputFilePath)
    {
        int replacementsMade = 0;
        using (StreamReader input = new StreamReader(inputFilePath))
        using (StreamWriter output = new StreamWriter(outputFilePath))
        {
            int character;
            while ((character = input.Read()) != -1) // Read character by character
            {
                // Replace quotation marks with spaces
                if (character == find)
                {
                    output.Write(replace);
                    replacementsMade++;
                }
                else
                {
                    output.Write((char)character);
                }
            }
        }
        return replacementsMade;
    }

    public static int FindCharReplaceString(char find, string replace, string inputFilePath, string outputFilePath)
    {
        int replacementsMade = 0;
        using (StreamReader input = new StreamReader(inputFilePath))
        using (StreamWriter output = new StreamWriter(outputFilePath))
        {
            int character;
            while ((character = input.Read()) != -1) // Read character by character
            {
                // Replace quotation marks with spaces
                if (character == find)
                {
                    output.Write(replace);
                    replacementsMade++;
                }
                else
                {
                    output.Write((char)character);
                }
            }
        }
        return replacementsMade;
    }

    public static int FindStringReplaceChar(string find, char replace, string inputFilePath, string outputFilePath)
    {
        int[] buffer = new int[find.Length]; // This is the window for storing characters being compared to the find string
        int index = 0; // Where in the window is currently being written to
        int replacementsMade = 0;

        using (StreamReader input = new StreamReader(inputFilePath))
        using (StreamWriter output = new StreamWriter(outputFilePath))
        {
            while ((buffer[index] = input.Read()) != -1) // Read character by character
            {
                if (buffer[index] == find[index]) // Checking current character for a match
                {
                    index++;
                    if (index == find.Length) // If the index reaches the end of the find string, we have a match.
                    {
                        index = 0;
                        output.Write(replace);
                        replacementsMade++;
                    }
                }
                else if (index > 0) // Earlier matches were a false positive. Write what was written to the buffer
                {
                    for (int i = 0; i < index; i++)
                    {
                        output.Write((char)buffer[i]);
                    }

                    index = 0;
                }
                else
                {
                    output.Write((char)buffer[index]);
                }
            }

            if (index > 0) // Reached end of file while attempting a match. Flush buffer
            {
                for (int i = 0; i < index; i++)
                {
                    output.Write((char)buffer[i]);
                }
            }
        }
        return replacementsMade;
    }
    
    public static int FindStringReplaceString(string find, string replace, string inputFilePath, string outputFilePath)
    {
        int[] buffer = new int[find.Length]; // This is the window for storing characters being compared to the find string
        int index = 0; // Where in the window is currently being written to
        int replacementsMade = 0;

        using (StreamReader input = new StreamReader(inputFilePath))
        using (StreamWriter output = new StreamWriter(outputFilePath))
        {
            while ((buffer[index] = input.Read()) != -1) // Read character by character
            {
                if (buffer[index] == find[index]) // Checking current character for a match
                {
                    index++;
                    if (index == find.Length) // If the index reaches the end of the find string, we have a match.
                    {
                        index = 0;
                        output.Write(replace);
                        replacementsMade++;
                    }
                }
                else if (index > 0) // Earlier matches were a false positive. Write what was written to the buffer
                {
                    for (int i = 0; i < index; i++)
                    {
                        output.Write((char)buffer[i]);
                    }

                    index = 0;
                }
                else
                {
                    output.Write((char)buffer[index]);
                }
            }

            if (index > 0) // Reached end of file while attempting a match. Flush buffer
            {
                for (int i = 0; i < index; i++)
                {
                    output.Write((char)buffer[i]);
                }
            }
        }
        return replacementsMade;
    }
}