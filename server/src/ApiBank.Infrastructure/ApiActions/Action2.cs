using ApiBank.Core.Attributes;
using ApiBank.Core.Enums;
using ApiBank.Core.Interfaces;

namespace ApiBank.Infrastructure.ApiActions;

[ServiceAction(ServiceNames.ServiceA)]
public class Action2 : IApiAction
{
    public Task ExecuteAsync()
    {
        return Task.CompletedTask;
    }
}