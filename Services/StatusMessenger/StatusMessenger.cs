using System;
using System.Collections.Generic;

namespace FixedLengthFile_Cleaner.Services.StatusMessenger;

/// <summary>
/// An asynchronous messenger for updating the current status and progress of the application/current process.
/// </summary>
public class StatusMessenger
{
    /// <summary>
    /// Event handler for <see cref="StatusEvent"/> events created by <see cref="SetStatus"/>.
    /// </summary>
    public event EventHandler<StatusEvent> StatusChanged;

    /// <summary>
    /// Event handler for <see cref="ProgressEvent"/> events created by <see cref="SetProgress"/>.
    /// </summary>
    public event EventHandler<ProgressEvent> ProgressChanged;

    private static StatusMessenger _instance;

    private StatusMessenger()
    {
    }

    public static StatusMessenger GetInstance()
    {
        if (_instance == null)
        {
            _instance = new StatusMessenger();
        }

        return _instance;
    }

    public void SetStatus(string? currentStatus)
    {
        var statusEvent = new StatusEvent(currentStatus);
        StatusChanged?.Invoke(this, statusEvent);
    }

    public void SetProgress(string? progress)
    {
        var progressEvent = new ProgressEvent(progress);
        ProgressChanged?.Invoke(this, progressEvent);
    }
}