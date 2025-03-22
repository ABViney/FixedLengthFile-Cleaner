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
using FixedLengthFile_Cleaner.Services;
using Serilog;

namespace FixedLengthFile_Cleaner;

public partial class MainWindow : Window
{
    private string _defaultInputFileTextBoxContent = "Input file goes here";
    private string _defaultOutputFileTextBoxContent = "Output file goes here";

    public Cleanable? SelectedCleanable { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        AddHandler(DragDrop.DropEvent, OnDrop);
        OutputPathTextBox.TextChanged += HandleOutputPathTextBoxTextChanged;

        Reset();
    }

    ///////////
    /// Methods
    ///////////
    private void Reset()
    {
        // Clear selected file and rest UI to starting state 
        SelectedCleanable = null;

        InputPathTextBox.Text = _defaultInputFileTextBoxContent;

        OutputPathTextBox.Text = _defaultOutputFileTextBoxContent;
        OutputPathTextBox.IsEnabled = false;
        OutputPathTextBox.IsReadOnly = true;

        OutputPathDialogButton.IsEnabled = false;

        CleanButton.IsEnabled = false;
        CleanButton.Content = "Clean";

        DropzoneDecalPresenter.ShowDropzone();
    }

    private void SetInputFile(string inputPath)
    {
        Log.Logger.Information($"Input set to {inputPath}");
        
        if (!Path.Exists(inputPath))
        {
            Log.Logger.Error($"{inputPath} does not exist");
            return;
        }

        SelectedCleanable = App.FetchService<CleanableFactory>().Create(inputPath);

        Dispatcher.UIThread.Post(() =>
        {
            // Updating the UI to indicate that a file is selected
            InputPathTextBox.Text = SelectedCleanable.InputPath;
            OutputPathTextBox.Text = SelectedCleanable.OutputPath;

            // Let the user change the output filename (InputFileTextBox is already readonly)
            OutputPathTextBox.IsReadOnly = false;
            OutputPathTextBox.IsEnabled = true;

            // Move carets to the end, so the end of the file name is in frame
            InputPathTextBox.SelectionStart = InputPathTextBox.SelectionEnd = InputPathTextBox.Text.Length;
            OutputPathTextBox.SelectionStart = OutputPathTextBox.SelectionEnd = OutputPathTextBox.Text.Length;

            // Select the default suffix of the output file and give it focus so the user can edit it immediately
            OutputPathTextBox.SelectionStart = Path.ChangeExtension(SelectedCleanable.InputPath, null).Length;
            OutputPathTextBox.SelectionEnd = Path.ChangeExtension(SelectedCleanable.OutputPath, null).Length;
            OutputPathTextBox.Focus();

            // Indicate to user that program is ready to proceed
            OutputPathDialogButton.IsEnabled = true;
            CleanButton.IsEnabled = true;
            CleanButton.Content = "Clean";

            // Update the decal to show what type of file is loaded
            if (SelectedCleanable.Type == CleanableType.Folder)
            {
                Log.Logger.Information("Folder selected");
                
                DropzoneDecalPresenter.ShowFolderReady();
                StatusBarControl.SetStatus("Folder ready", null);
            }
            else if (SelectedCleanable.Type == CleanableType.ZipFile)
            {
                Log.Logger.Information("Zip file selected");
                
                DropzoneDecalPresenter.ShowZipArchiveReady();
                StatusBarControl.SetStatus("Zip archive ready", null);
            }
            else
            {
                Log.Logger.Information("Text file selected");
                
                DropzoneDecalPresenter.ShowSingleFileReady();
                StatusBarControl.SetStatus("File ready", null);
            }
        });
    }

    ///////////////////
    /// Event Handlers
    ///////////////////
    private async void HandleInputPathButtonClick(object sender, RoutedEventArgs e)
    {
        // Note: Default picker does not support selecting both folders and files.
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

    private async void HandleOutputPathButtonClick(object? sender, RoutedEventArgs e)
    {
        // Note: Default picker does not support selecting both folders and files.
        if (SelectedCleanable is null) return;

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Set save file location",
            ShowOverwritePrompt = true,
            SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(SelectedCleanable.InputPath)
        });

        if (file is null) return;

        OutputPathTextBox.Text = file.TryGetLocalPath() ?? string.Empty;
    }

    private async void HandleOutputPathTextBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (SelectedCleanable is null || String.IsNullOrEmpty(OutputPathTextBox.Text)) return;
        SelectedCleanable.OutputPath = OutputPathTextBox.Text;
        
        Log.Logger.Information($"Output path changed to {SelectedCleanable.OutputPath}");
    }

    private async void OnCleanButtonClick(object sender, RoutedEventArgs e)
    {
        if (!Path.Exists(SelectedCleanable.InputPath))
        {
            Log.Logger.Error($"Error: {SelectedCleanable.InputPath} does not exist.");
            Reset();
            StatusBarControl.ResetStatus();
            return;
        }

        Configuration config = App.FetchService<Configuration>();
        Cleaner cleaner = App.FetchService<Cleaner>();

        Dispatcher.UIThread.Post(() =>
        {
            CleanButton.Content = "Cleaning...";
            CleanButton.IsEnabled = false;
            DropzoneDecalPresenter.ShowProcessing();
            OutputPathTextBox.IsEnabled = false;
            OutputPathDialogButton.IsEnabled = false;
        });

        // Decompress the archive and set the files' output target to the cleaned directory
        Dispatcher.UIThread.Post(() =>
        {
            StatusBarControl.SetStatus($"Processing {Path.GetFileName(SelectedCleanable.InputPath)}...", null);
        });

        try
        {
            Log.Logger.Information($"Cleaning {SelectedCleanable.InputPath}...");
            await cleaner.Clean(SelectedCleanable);
            Log.Logger.Information($"Finished cleaning {SelectedCleanable.InputPath}.");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, $"Error while cleaning {SelectedCleanable.InputPath}:");
        }
        
        
        // Reset view and inform user of how many quotes were replaced.
        Dispatcher.UIThread.Post(() =>
        {
            StatusBarControl.SetStatus($"Replaced {SelectedCleanable.NumberOfQuotes}", "Done");
            Reset();
        });

        Log.Logger.Information("Cleaning process completed.");
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        IStorageItem[] storageItems = e.Data.GetFiles().ToArray();
        if (storageItems.Length == 1)
        {
            Log.Logger.Information($"File dropped into application: {storageItems[0].TryGetLocalPath()}");
            SetInputFile(storageItems[0].TryGetLocalPath());
        }
    }
}