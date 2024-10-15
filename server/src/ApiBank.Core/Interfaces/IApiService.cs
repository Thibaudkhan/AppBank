namespace ApiBank.Core.Interfaces;

public interface IApiService
{
    string ServiceName { get; }

    IEnumerable<string> GetAvailableActions();

    Task<Guid> ExecuteActionAsync(string actionName);

    string GetLogs(Guid taskId);

}