using System.Collections.Concurrent;
using System.Text;
using ApiBank.Shared.DTOs;
using ApiBank.Core.Interfaces;

namespace ApiBank.Infrastructure.Services;


public class InMemoryLoggingService : ILoggingService
{
    private readonly ConcurrentDictionary<Guid, List<string>> _logs = new();

    public event EventHandler<LogEventArgs> LogAdded;

    public void Log(Guid taskId, string message)
    {
        var logList = _logs.GetOrAdd(taskId, new List<string>());
        lock (logList)
        {
            logList.Add(message);
        }

        LogAdded?.Invoke(this, new LogEventArgs
        {
            TaskId = taskId,
            LogMessage = message
        });
    }

    public string GetLogs(Guid taskId)
    {
        if (_logs.TryGetValue(taskId, out var logList))
        {
            lock (logList)
            {
                return string.Join(Environment.NewLine, logList);
            }
        }
        return string.Empty;
    }


    public void ClearLogs(Guid taskId)
    {
        _logs.TryRemove(taskId, out _);
    }
}