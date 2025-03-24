using System;
using System.Collections.Generic;

namespace FixedLengthFile_Cleaner.Services.StatusMessenger;

public class StatusMessenger
{
    public event EventHandler<StatusEvent> StatusChanged;
    public event EventHandler<ProgressEvent> ProgressChanged;
    
    private static StatusMessenger _instance;
    
    private StatusMessenger() { }

    
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