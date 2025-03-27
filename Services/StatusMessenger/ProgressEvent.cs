using System;

namespace FixedLengthFile_Cleaner.Services.StatusMessenger;

/// <summary>
/// Indicates a change in progress of the currently running process.
/// </summary>
public class ProgressEvent : EventArgs
{
    public string? Progress { get; }
    public ProgressEvent(string? progress) => Progress = progress;
}