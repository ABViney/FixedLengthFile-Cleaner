using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FixedLengthFile_Cleaner.Models;

namespace FixedLengthFile_Cleaner;

public partial class MainWindow : Window
{
    const string DEFAULT_OUTPUT_FILENAME_SUFFIX = "_cleaned"; 
    

    private string _defaultInputFileTextBoxContent = "Input file goes here";
    private string _defaultOutputFileTextBoxContent = "Output file goes here";
    
    public CleanableFile? SelectedFile { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        AddHandler(DragDrop.DropEvent, OnDrop);
        
        Reset();
    }
    
    ///////////
    /// Methods
    ///////////

    private Task CleanTextFile(CleanableFile textFile)
    {
        return Task.Run(() =>
        {
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
                            textFile.NumberOfQuotes++;
                            character = ' ';
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
    
    private void Reset()
    {
        // Clear selected file and rest UI to starting state 
        SelectedFile = null;
        
        InputFileTextBox.Text = _defaultInputFileTextBoxContent;
        
        OutputFileTextBox.Text = _defaultOutputFileTextBoxContent;
        OutputFileTextBox.IsEnabled = false;
        OutputFileTextBox.IsReadOnly = true;
        
        OutputFileDialogButton.IsEnabled = false;
        
        CleanButton.IsEnabled = false;
        
        DropzoneDecalPresenter.ShowDropzone();
    }
    
    public void SelectFile(string inputFilePath)
    {
        if (!Path.Exists(inputFilePath))
        {
            Console.WriteLine("File does not exist");
            return;
        }

        // change the output file path so it doesn't overwrite the original
        string outputFilePath;
        string extension = Path.GetExtension(inputFilePath);
        if (extension == String.Empty)
        {
            outputFilePath = inputFilePath + DEFAULT_OUTPUT_FILENAME_SUFFIX;
        }
        else
        {
            // If the file has an extension, insert the suffix before it.
            outputFilePath = String.Concat(Path.ChangeExtension(inputFilePath, null), DEFAULT_OUTPUT_FILENAME_SUFFIX, extension);
        }

        SelectedFile = new CleanableFile
        {
            InputFilePath = inputFilePath,
            OutputFilePath = outputFilePath,
            FileType = inputFilePath.EndsWith("zip") ? CleanableFileType.ZipFile : CleanableFileType.TextFile
        };
    }

    private void SetInputFile(string inputFilePath)
    {
        SelectFile(inputFilePath);

        if (SelectedFile is null)
        {
            return;
        }
            
        // Updating the UI to indicate that a file is selected
        InputFileTextBox.Text = SelectedFile.InputFilePath;
        OutputFileTextBox.Text = SelectedFile.OutputFilePath;
        
        // Let the user change the output filename (InputFileTextBox is already readonly)
        OutputFileTextBox.IsReadOnly = false;
        OutputFileTextBox.IsEnabled = true;

        // Move carets to the end, so the end of the file name is in frame
        InputFileTextBox.SelectionStart = InputFileTextBox.SelectionEnd = InputFileTextBox.Text.Length;
        OutputFileTextBox.SelectionStart = OutputFileTextBox.SelectionEnd = OutputFileTextBox.Text.Length;

        // Select the default suffix of the output file and give it focus so the user can edit it immediately
        OutputFileTextBox.SelectionStart = Path.ChangeExtension(SelectedFile.InputFilePath, null).Length;
        OutputFileTextBox.SelectionEnd = OutputFileTextBox.SelectionStart + DEFAULT_OUTPUT_FILENAME_SUFFIX.Length;
        OutputFileTextBox.Focus();

        // Indicate to user that program is ready to proceed
        OutputFileDialogButton.IsEnabled = true;
        CleanButton.IsEnabled = true;
        CleanButton.Content = "Clean";
        
        // Update the decal to show what type of file is loaded
        if (SelectedFile.FileType == CleanableFileType.ZipFile)
        {
            DropzoneDecalPresenter.ShowZipArchiveReady();
        }
        else
        {
            DropzoneDecalPresenter.ShowSingleFileReady();
        }
    }
    
    ///////////////////
    /// Event Handlers
    ///////////////////
    
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
        if (SelectedFile is null) return;
        
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Set save file location",
            ShowOverwritePrompt = true,
            SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(SelectedFile.InputFilePath)
        });

        if (file is null) return;

        SelectedFile.OutputFilePath = file.TryGetLocalPath() ?? String.Empty;
    }

    private async void OnCleanButtonClick(object sender, RoutedEventArgs e)
    {
        if (!Path.Exists(SelectedFile.InputFilePath))
        {
            Console.WriteLine("Error: File does not exist");
            Reset();
            return;
        }

        if (SelectedFile.FileType == CleanableFileType.TextFile)
        {
            // Update view to indicate cleaning
            Dispatcher.UIThread.Post(() =>
            {
                CleanButton.Content = "Cleaning...";
                DropzoneDecalPresenter.ShowProcessing();
            });
            await CleanTextFile(SelectedFile);
        }

        if (SelectedFile.FileType == CleanableFileType.ZipFile)
        {
            // Unpack zip file to temporary location
            // Clean each file
            // Repackage zip to output destination
            // Delete temporary files
        }
        
        // Reset view and inform user of how many quotes were replaced.
        Dispatcher.UIThread.Post(() =>
        {
            CleanButton.Content =
                $"{SelectedFile.NumberOfQuotes} \"{(SelectedFile.NumberOfQuotes == 1 ? "" : "s")} replaced";
            Reset();
        });
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
    
}