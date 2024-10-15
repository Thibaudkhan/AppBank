using ApiBank.Core.Attributes;
using ApiBank.Core.Enums;
using ApiBank.Core.Interfaces;

namespace ApiBank.Infrastructure.ApiActions;

[ServiceAction(ServiceNames.ServiceB)]
public class Action3 : IApiAction
{
    public Task ExecuteAsync()
    {
        return Task.CompletedTask;
    }
}