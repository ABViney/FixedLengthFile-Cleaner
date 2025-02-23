using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace FixedLengthFile_Cleaner;

public partial class MainWindow : Window
{
    const string DEFAULT_OUTPUT_FILENAME_SUFFIX = "_cleaned"; 
    
    private string? _inputFilePath;
    private string? _outputFilePath;
    private string? _inputFileType;

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
                _inputFileType = null;
                InputFileTextBox.Text = _defaultInputFileTextBoxContent;
                OutputFilePath = null;
                return;
            }
            
            _inputFilePath = value;
            InputFileTextBox.Text = _inputFilePath;
            InputFileTextBox.CaretIndex = InputFileTextBox.Text.Length;
            
            // Add "_cleaned" to the file name
            string extension = Path.GetExtension(value);
            if (extension == String.Empty)
            {
                OutputFilePath = _inputFilePath + DEFAULT_OUTPUT_FILENAME_SUFFIX;
            }
            else
            {
                // If the file has an extension, insert the suffix before it.
                OutputFilePath = String.Concat(Path.ChangeExtension(value, null), DEFAULT_OUTPUT_FILENAME_SUFFIX, extension);
            }
            
            _inputFileType = extension.EndsWith("zip") ? "zip" : "text";
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
                OutputFileTextBox.IsReadOnly = true;
                OutputFileDialogButton.IsEnabled = false;
                CleanButton.IsEnabled = false;
                return;
            }
            
            _outputFilePath = value;
            OutputFileTextBox.Text = _outputFilePath;
            
            // Enable editing so the user can change the output filename easily
            OutputFileTextBox.IsEnabled = true;
            OutputFileTextBox.IsReadOnly = false;
            
            // Move caret to the end, so the end of the file name is in frame
            OutputFileTextBox.SelectionStart = OutputFileTextBox.SelectionEnd = OutputFileTextBox.Text.Length;
            
            // then move the cursor behind the extension if there is one, and select the text
            // OutputFileTextBox.CaretIndex = OutputFileTextBox.Text.Length - Path.GetExtension(OutputFileTextBox.Text).Length;
            OutputFileTextBox.SelectionStart = OutputFileTextBox.Text.Length
                                               - Path.GetExtension(value).Length
                                               - DEFAULT_OUTPUT_FILENAME_SUFFIX.Length;
            OutputFileTextBox.SelectionEnd = OutputFileTextBox.Text.Length
                                             - Path.GetExtension(value).Length;
            
            // Give the control focus so they user can type immediately
            OutputFileTextBox.Focus();
            
            OutputFileDialogButton.IsEnabled = true;
            CleanButton.IsEnabled = true;
            CleanButton.Content = "Clean";
            DropzoneDecalPresenter.ShowSingleFileReady();
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
            SetInputFile(files[0].TryGetLocalPath());
        }
    }
    
    private async void HandleOutputFileButtonClick(object? sender, RoutedEventArgs e)
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
            SetInputFile(files[0].TryGetLocalPath());
        }
    }

    private async void OnCleanButtonClick(object sender, RoutedEventArgs e)
    {
        // Run process on not the UI thread
        await Task.Run(() =>
        {
            try
            {
                using (StreamReader input = new StreamReader(InputFilePath))
                using (StreamWriter output = new StreamWriter(OutputFilePath))
                {
                    // Update view to indicate cleaning
                    Dispatcher.UIThread.Post(() =>
                    {
                        CleanButton.Content = "Cleaning...";
                        DropzoneDecalPresenter.ShowProcessing();
                    });

                    int character;
                    int numOfQuotesReplaced = 0;
                    while ((character = input.Read()) != -1) // Read character by character
                    {
                        // Replace quotation marks with spaces
                        if (character == '"')
                        {
                            character = ' ';
                            numOfQuotesReplaced++;
                        }

                        output.Write((char)character);
                    }

                    // Reset view and inform user of how many quotes were replaced.
                    Dispatcher.UIThread.Post(() =>
                    {
                        CleanButton.Content =
                            $"{numOfQuotesReplaced} \"{(numOfQuotesReplaced == 1 ? "" : "s")} replaced";
                        Reset();
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        });
    }
    
    private void Reset()
    {
        InputFilePath = null;
        DropzoneDecalPresenter.ShowDropzone();
    }

    private void SetInputFile(string inputFilePath)
    {
        InputFilePath = inputFilePath;
        if (_inputFileType == "zip")
        {
            DropzoneDecalPresenter.ShowZipArchiveReady();
        }
        else
        {
            DropzoneDecalPresenter.ShowSingleFileReady();
        }
    }
}