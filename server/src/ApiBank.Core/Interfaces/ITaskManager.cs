namespace ApiBank.Core.Interfaces;

public interface ITaskManager
{
    Task<Guid> EnqueueTaskAsync(Func<CancellationToken, Task> taskFunc);
}