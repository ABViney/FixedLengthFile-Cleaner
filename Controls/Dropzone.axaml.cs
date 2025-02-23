using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace FixedLengthFile_Cleaner.Controls;

public partial class DropzoneDecalPresenter : UserControl
{
    
    
    public DropzoneDecalPresenter()
    {
        InitializeComponent();
    }
    
    public void ShowDropzone()
    {
        Dispatcher.UIThread.Post(() =>
        {
            FileDropzoneDecalImage.IsVisible = true;
            
            SingleFileReadyDecalImage.IsVisible = false;
            ZipArchiveReadyDecalImage.IsVisible = false;
            ProcessingDecalImage.IsVisible = false;
        });
    }
    
    public void ShowSingleFileReady()
    {
        FileDropzoneDecalImage.IsVisible = false;
        
        SingleFileReadyDecalImage.IsVisible = true;
        
        ZipArchiveReadyDecalImage.IsVisible = false;
        ProcessingDecalImage.IsVisible = false;
    }
    
    public void ShowZipArchiveReady()
    {
        FileDropzoneDecalImage.IsVisible = false;
        SingleFileReadyDecalImage.IsVisible = false;
        
        ZipArchiveReadyDecalImage.IsVisible = true;
        
        ProcessingDecalImage.IsVisible = false;
    }
    
    public void ShowProcessing()
    {
        FileDropzoneDecalImage.IsVisible = false;
        SingleFileReadyDecalImage.IsVisible = false;
        ZipArchiveReadyDecalImage.IsVisible = false;
        
        ProcessingDecalImage.IsVisible = true;
    }
}