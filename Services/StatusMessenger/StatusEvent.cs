using System;

namespace FixedLengthFile_Cleaner.Services.StatusMessenger;

public class StatusEvent : EventArgs
{
    public string? Message { get; }

    public StatusEvent(string? message) => Message = message;
}