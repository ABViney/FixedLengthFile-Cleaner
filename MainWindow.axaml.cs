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
    
    public string? InputFilePath
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
            InputFileTextBox.Text = value;

            string default_output_file_path;
            
            // Add "_cleaned" to the file name
            if (Path.HasExtension(value)) // If the file has an extension, insert the suffix before it.
            {
                string extension = Path.GetExtension(value);
                default_output_file_path = String.Concat(Path.ChangeExtension(value, null), DEFAULT_OUTPUT_FILENAME_SUFFIX, extension);
            }
            else
            {
                default_output_file_path = value + DEFAULT_OUTPUT_FILENAME_SUFFIX;
            }

            OutputFilePath = default_output_file_path;
        }
    }
    
    public string? OutputFilePath
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
            OutputFileTextBox.Text = _outputFilePath;
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
            InputFilePath = files[0].TryGetLocalPath();
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

        OutputFilePath = file.TryGetLocalPath();
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        IStorageItem[] files = e.Data.GetFiles().ToArray();
        if (files.Length == 1)
        {
            Console.WriteLine($"Dropped file {files[0]}");
            InputFilePath = files[0].TryGetLocalPath();
        }
    }

    private void OnCleanButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            using (StreamReader input = new StreamReader(InputFilePath))
            using (StreamWriter output = new StreamWriter(OutputFilePath))
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