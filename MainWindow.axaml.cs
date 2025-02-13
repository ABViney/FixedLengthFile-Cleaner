using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace FixedLengthFile_Cleaner;

public partial class MainWindow : Window
{
    const string DEFAULT_OUTPUT_FILENAME_SUFFIX = "_cleaned"; 
    
    private string? _inputFilePath;
    private string? _outputFilePath;

    private string _defaultInputFileTextBoxContent = "Input file goes here";
    private string _defaultOutputFileTextBoxContent = "Output file goes here";
    
    public Uri? InputFilePath
    {
        get => _inputFilePath;
        set // Sets the input file path as well as the output file path and the input file text box
        {
            if (value is null) // Reset to default
            {
                _inputFilePath = null;
                InputFileTextBox.Text = _defaultInputFileTextBoxContent;
                OutputFilePath = null;
                return;
            }
            
            _inputFilePath = value;
            InputFileTextBox.Text = Uri.UnescapeDataString(_inputFilePath.AbsolutePath);
            

            Uri default_output_path;

            
            // Add "_cleaned" to the file name
            if (value.ToString().Contains(".")) // If the file has an extension, insert the suffix before it.
            {
                string[] split = Uri.UnescapeDataString(_inputFilePath.AbsolutePath).Split('.');
                split[split.Length - 2] = split[split.Length - 2] + default_output_suffix;
                default_output_path = new Uri(String.Join(".", split));
            }
            else
            {
                default_output_path = new Uri(value + default_output_suffix);
            }

            OutputFilePath = default_output_path;
        }
    }
    
    public Uri? OutputFilePath
    {
        get => _outputFilePath;
        set
        {
            if (value is null) // Reset to default and disable
            {
                _outputFilePath = null;
                OutputFileTextBox.Text = _defaultOutputFileTextBoxContent;
                OutputFileTextBox.IsEnabled = false;
                OutputFileDialogButton.IsEnabled = false;
                CleanButton.IsEnabled = false;
                return;
            }
            
            _outputFilePath = value;
            OutputFileTextBox.Text = Uri.UnescapeDataString(_outputFilePath.AbsolutePath);
            OutputFileTextBox.IsEnabled = true;
            OutputFileDialogButton.IsEnabled = true;
            CleanButton.IsEnabled = true;
        }
    }
    
    public MainWindow()
    {
        InitializeComponent();
        AddHandler(DragDrop.DropEvent, OnDrop);
        
        Reset();
    }

    private async void HandleInputFileButtonClick(object sender, RoutedEventArgs e)
    {
        var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Select input file",
            AllowMultiple = false,
            SuggestedStartLocation = await this.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents),
        });

        if (files.Count == 1)
        {
            InputFilePath = files[0].Path;
        }
    }

    private async void HandleOutputFileButtonClick(object sender, RoutedEventArgs e)
    {
        var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Set save file location",
            ShowOverwritePrompt = true,
            SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(InputFilePath)
        });

        OutputFilePath = file.Path;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        IStorageItem[] files = e.Data.GetFiles().ToArray();
        if (files.Length == 1)
        {
            Console.WriteLine($"Dropped file {files[0]}");
            InputFilePath = files[0].Path;
        }
    }

    private void OnCleanButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            using (StreamReader input = new StreamReader(Uri.UnescapeDataString(InputFilePath.AbsolutePath)))
            using (StreamWriter output = new StreamWriter(Uri.UnescapeDataString(OutputFilePath.AbsolutePath)))
            {
                int character;
                while ((character = input.Read()) != -1) // Read character by character
                {
                    if (character == '"')
                    {
                        // Replace quotation marks with spaces
                        character = ' ';
                    }

                    output.Write((char)character);
                }
            }
            
            Reset();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
    }

    private void Reset()
    {
        InputFilePath = null;
    }
}