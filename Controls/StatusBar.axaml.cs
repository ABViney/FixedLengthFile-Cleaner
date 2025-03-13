using Avalonia.Controls;

namespace FixedLengthFile_Cleaner.Controls;

public partial class StatusBar : UserControl
{
    private string _defaultCurrentActionText = "Awaiting input...";
    
    public StatusBar()
    {
        InitializeComponent();
        ResetStatus();
    }

    public void ResetStatus()
    {
        SetStatus(null, null);
    }
    
    public void SetStatus(string? currentStatus, string? progressIndicator)
    {
        CurrentStatusTextBlock.Text = currentStatus ?? _defaultCurrentActionText;
        CurrentProgressTextBlock.Text = progressIndicator ?? "";
    }
}