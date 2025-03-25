using System;

namespace FixedLengthFile_Cleaner.Services.StatusMessenger;

public class ProgressEvent : EventArgs
{
    public string? Progress { get; }
    public ProgressEvent(string? progress) => Progress = progress;
}