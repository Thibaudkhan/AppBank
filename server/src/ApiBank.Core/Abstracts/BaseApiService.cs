using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using ApiBank.Core.Attributes;
using ApiBank.Core.Interfaces;

namespace ApiBank.Core.Abstracts;

public abstract class BaseApiService : IApiService
{
    public abstract string ServiceName { get; }

    protected readonly Dictionary<string, IApiAction> _actions = new Dictionary<string, IApiAction>();
    private readonly ITaskManager _taskManager;
    private readonly ILoggingService _loggingService;

    protected BaseApiService(IEnumerable<IApiAction> actions, ITaskManager taskManager, ILoggingService loggingService)
    {
        _taskManager = taskManager;
        _loggingService = loggingService;
        InitializeActions(actions);
    }

    private void InitializeActions(IEnumerable<IApiAction> actions)
    {
        foreach (var action in actions)
        {
            var attribute = action.GetType().GetCustomAttribute<ServiceActionAttribute>();
            if (attribute != null && attribute.ServiceName == ServiceName)
            {
                _actions.Add(action.GetType().Name, action);
            }
        }
    }

    public IEnumerable<string> GetAvailableActions()
    {
        return _actions.Keys;
    }

    public virtual async Task<Guid> ExecuteActionAsync(string actionName)
    {
        if (_actions.TryGetValue(actionName, out var action))
        {
            var taskId = Guid.NewGuid();

            Func<CancellationToken, Task> taskFunc = async (cancellationToken) =>
            {
                try
                {
                    var startTime = DateTime.UtcNow;
                    var endTime = startTime.AddSeconds(5);
                    var actionTask = action.ExecuteAsync();

                    while (DateTime.UtcNow < endTime)
                    {
                        var logLine = $"[{DateTime.UtcNow:O}] {GenerateRandomLogLine()}";
                        _loggingService.Log(taskId, logLine);

                        await Task.Delay(1000, cancellationToken);
                    }
                    await actionTask;
                    
                    

                    _loggingService.Log(taskId, $"[{DateTime.UtcNow:O}] Action terminée avec succès.");
                }
                catch (OperationCanceledException)
                {
                    _loggingService.Log(taskId, $"[{DateTime.UtcNow:O}] La tâche a été annulée.");
                }
                catch (Exception ex)
                {
                    _loggingService.Log(taskId, $"[{DateTime.UtcNow:O}] Erreur: {ex.Message}");
                }
            };

            _taskManager.EnqueueTaskAsync(taskFunc);

            return taskId;
        }
        else
        {
            throw new ArgumentException($"Action {actionName} non trouvée");
        }
    }

    public string GetLogs(Guid taskId)
    {
        return _loggingService.GetLogs(taskId);
    }

    private string GenerateRandomLogLine()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        int length = random.Next(20, 100); // Génère une longueur aléatoire entre 20 et 100 caractères
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
    }

}