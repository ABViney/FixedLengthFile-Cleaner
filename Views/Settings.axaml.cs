using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FixedLengthFile_Cleaner.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    public override void Show()
    {
        base.Show();
        
        // Populate controls with current config values
    }
}