using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using FixedLengthFile_Cleaner.Services.StatusMessenger;

namespace FixedLengthFile_Cleaner.Controls;

public partial class StatusBar : UserControl
{
    private string _defaultCurrentActionText = "Awaiting input...";
    private string _defaultProgressText = "";
    
    public StatusBar()
    {
        InitializeComponent();
        CurrentStatusTextBlock.Text = _defaultCurrentActionText;
        CurrentProgressTextBlock.Text = "";
    }

    public void OnProgressChanged(object?  sender, ProgressEvent progressEvent)
    {
        Dispatcher.UIThread.Post(() => CurrentProgressTextBlock.Text = progressEvent.Progress ?? _defaultProgressText);

    }
    
    public void OnStatusChanged(object? sender, StatusEvent statusEvent)
    {
        Dispatcher.UIThread.Post(() =>CurrentStatusTextBlock.Text = statusEvent.Message ?? _defaultCurrentActionText);
    }
}