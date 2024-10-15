using System.Reflection;
using ApiBank.Core.Abstracts;
using ApiBank.Core.Attributes;
using ApiBank.Core.Enums;
using ApiBank.Core.Interfaces;
using ApiBank.Infrastructure.Registries;

namespace ApiBank.Infrastructure.ApiServices;

public class ApiServiceB : BaseApiService
{
    private readonly ServiceRegistry _serviceRegistry;
    public override string ServiceName => ServiceNames.ServiceB;

    public ApiServiceB(IEnumerable<IApiAction> actions, ITaskManager taskManager, ILoggingService loggingService, ServiceRegistry serviceRegistry)
        : base(actions, taskManager, loggingService)
    {
        _serviceRegistry = serviceRegistry;
    }

    public override async Task<Guid> ExecuteActionAsync(string actionName)
    {
        var taskId = await base.ExecuteActionAsync(actionName);
        
        _serviceRegistry.MapTaskToService(taskId, this);

        return taskId;
    }
}
