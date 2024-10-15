namespace ApiBank.Shared.DTOs;

public class LogEventArgs : EventArgs
{
    public Guid TaskId { get; set; }
    public string LogMessage { get; set; }
}