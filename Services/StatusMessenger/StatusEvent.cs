using System;

namespace FixedLengthFile_Cleaner.Services.StatusMessenger;

/// <summary>
/// An event for relaying the current status of the running process.
/// </summary>
public class StatusEvent : EventArgs
{
    public string? Message { get; }

    public StatusEvent(string? message) => Message = message;
}