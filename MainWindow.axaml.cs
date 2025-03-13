using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FixedLengthFile_Cleaner.Helpers;
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
        CleanButton.Content = "Clean";

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
                StatusBarControl.SetStatus("Zip archive ready", null);
            }
            else
            {
                DropzoneDecalPresenter.ShowSingleFileReady();
                StatusBarControl.SetStatus("File ready", null);
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
            StatusBarControl.ResetStatus();
            return;
        }

        Configuration config = App.FetchService<Configuration>();

        Dispatcher.UIThread.Post(() =>
        {
            CleanButton.Content = "Cleaning...";
            CleanButton.IsEnabled = false;
            DropzoneDecalPresenter.ShowProcessing();
        });

        ///////////////////
        // Clean text file
        ///////////////////
        if (SelectedFile.FileType == CleanableFileType.TextFile)
        {
            // Update view to indicate cleaning
            Dispatcher.UIThread.Post(() =>
            {
                StatusBarControl.SetStatus("Cleaning...", null);
            });
            await CleanableFile.Clean(SelectedFile);
            Dispatcher.UIThread.Post(() =>
            {
                StatusBarControl.SetStatus($"Replaced {SelectedFile.NumberOfQuotes}", "1/1");
            });
        }

        ///////////////////
        // Clean zip archive
        ///////////////////
        if (SelectedFile.FileType == CleanableFileType.ZipFile)
        {
            // Create directory for cleaned files to be written to
            ResourceManagement.CreateTemporaryDirectory();

            // Decompress the archive and set the files' output target to the cleaned directory
            Dispatcher.UIThread.Post(() =>
            {
                StatusBarControl.SetStatus($"Unzipping {Path.GetFileName(SelectedFile.InputFilePath)}...", null);
            });
            CleanableFile[] files = [];
            await Task.Run(() => files = ResourceManagement.DecompressZipFile(SelectedFile.InputFilePath).ToArray());

            for (int i = 0; i < files.Count(); i++)
            {
                var file = files[i];
                Dispatcher.UIThread.Post(() =>
                {
                    StatusBarControl.SetStatus($"Cleaning {Path.GetFileName(file.InputFilePath)}", $"{i}/{files.Count()}");
                });
                await CleanableFile.Clean(file);
                SelectedFile.NumberOfQuotes += file.NumberOfQuotes;
            }

            // Repackage zip to output destination
            Dispatcher.UIThread.Post(() =>
            {
                StatusBarControl.SetStatus($"Zipping {Path.GetFileName(SelectedFile.OutputFilePath)}...", null);
            });
            await Task.Run(() => ResourceManagement.CompressCleanedFiles(SelectedFile.OutputFilePath));

            // Delete temporary files
            Dispatcher.UIThread.Post(() => { StatusBarControl.SetStatus("Deleting temporary files...", null); });
            await Task.Run(() => ResourceManagement.DeleteTemporaryDirectory());

            Dispatcher.UIThread.Post(() =>
            {
                StatusBarControl.SetStatus($"Replaced {SelectedFile.NumberOfQuotes}", $"{files.Count()}/{files.Count()}");
            });
        }

        // Reset view and inform user of how many quotes were replaced.
        Dispatcher.UIThread.Post(() =>
        {
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