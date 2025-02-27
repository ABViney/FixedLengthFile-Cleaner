using System;
using System.IO;
using System.IO.Compression;
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

    private void SetInputFile(string inputFilePath)
    {
        if (!Path.Exists(inputFilePath))
        {
            Console.WriteLine("File does not exist");
            return;
        }

        SelectedFile = new CleanableFile(inputFilePath);

        Dispatcher.UIThread.Post(() =>
        {
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
            OutputFileTextBox.SelectionEnd = Path.ChangeExtension(SelectedFile.OutputFilePath, null).Length;
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
        });
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

        Configuration config = App.FetchService<Configuration>();
        
        Dispatcher.UIThread.Post(() =>
        {
            CleanButton.Content = "Cleaning...";
            DropzoneDecalPresenter.ShowProcessing();
        });

        ///////////////////
        // Clean text file
        ///////////////////
        if (SelectedFile.FileType == CleanableFileType.TextFile)
        {
            // Update view to indicate cleaning

            await CleanableFile.Clean(SelectedFile);
        }

        ///////////////////
        // Clean zip archive
        ///////////////////
        if (SelectedFile.FileType == CleanableFileType.ZipFile)
        {
            // Create directory for cleaned files to be written to
            ResourceManagement.CreateTemporaryDirectory();

            // Decompress the archive and set the files' output target to the cleaned directory
            
            var files = ResourceManagement.DecompressZipFile(SelectedFile.InputFilePath);
            
            foreach (var file in files)
            {
                await CleanableFile.Clean(file);
                SelectedFile.NumberOfQuotes += file.NumberOfQuotes;
            }

            // Repackage zip to output destination
            ResourceManagement.CompressCleanedFiles(SelectedFile.OutputFilePath);

            // Delete temporary files
            ResourceManagement.DeleteTemporaryDirectory();
        }

        // Reset view and inform user of how many quotes were replaced.
        Dispatcher.UIThread.Post(() =>
        {
            CleanButton.Content =
                $"{SelectedFile.NumberOfQuotes} \"{(SelectedFile.NumberOfQuotes == 1 ? "" : "s")} replaced";
            Reset();
        });
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        IStorageItem[] files = e.Data.GetFiles().ToArray();
        if (files.Length == 1)
        {
            Console.WriteLine($"Dropped file {files[0].TryGetLocalPath()}");
            SetInputFile(files[0].TryGetLocalPath());
        }
    }
}