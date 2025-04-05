using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FixedLengthFile_Cleaner.Models;
using FixedLengthFile_Cleaner.Services;

namespace FixedLengthFile_Cleaner.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        Reset();
    }

    private void Reset()
    {
        Configuration config = App.FetchService<Configuration>();
        Dispatcher.UIThread.Post(() =>
        {
            FindTextBox.Text = config.Find;
            ReplaceTextBox.Text = config.Replace;
            ExcludeFilesTextBox.Text = String.Join(',', config.ExcludePatterns);
            OutputSuffixTextBox.Text = config.OutputSuffix;
        });
    }

    private void OnApplyButtonClick(object sender, RoutedEventArgs e)
    {
        var config = new Configuration()
        {
            Find = FindTextBox.Text,
            Replace = ReplaceTextBox.Text,
            ExcludePatterns = ExcludeFilesTextBox.Text.Split(','),
            OutputSuffix = OutputSuffixTextBox.Text
        };
        var cm = App.FetchService<ConfigurationManager>();
        cm.UpdateConfiguration(config);
        
        Hide();
    }

    private void OnCancelButtonClick(object sender, RoutedEventArgs e)
    {
        Reset();
        Hide();
    }
    
}