using ApiBank.Shared.DTOs;

namespace ApiBank.Core.Interfaces;

public interface ILoggingService
{
    
    void Log(Guid taskId, string message);

    string GetLogs(Guid taskId);

    void ClearLogs(Guid taskId);

    event EventHandler<LogEventArgs> LogAdded;
}