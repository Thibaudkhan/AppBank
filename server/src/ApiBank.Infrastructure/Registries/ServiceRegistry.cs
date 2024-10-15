using System.Collections.Concurrent;
using System.Text;
using ApiBank.Core.Interfaces;

namespace ApiBank.Infrastructure.Registries;

public class ServiceRegistry
{
    private readonly Dictionary<string, IApiService> _services = new Dictionary<string, IApiService>();
    private readonly Dictionary<Guid, IApiService> _taskServiceMap = new Dictionary<Guid, IApiService>();

    public void RegisterService(IApiService service)
    {
        if (string.IsNullOrEmpty(service.ServiceName))
        {
            throw new ArgumentException("Le ServiceName ne peut pas être nul ou vide.");
        }

        if (!_services.ContainsKey(service.ServiceName))
        {
            _services.Add(service.ServiceName, service);
        }
        else
        {
            throw new ArgumentException($"Le service {service.ServiceName} est déjà enregistré.");
        }
    }

    public IEnumerable<string> GetAllServiceNames()
    {
        return _services.Keys;
    }

    public IEnumerable<string> GetServiceActions(string serviceName)
    {
        if (_services.TryGetValue(serviceName, out var service))
        {
            return service.GetAvailableActions();
        }
        throw new ArgumentException($"Service {serviceName} non trouvé.");
    }

    public IApiService GetService(string serviceName)
    {
        if (_services.TryGetValue(serviceName, out var service))
        {
            return service;
        }
        throw new ArgumentException($"Service {serviceName} non trouvé.");
    }
    
    public void MapTaskToService(Guid taskId, IApiService service)
    {
        _taskServiceMap[taskId] = service;
    }

    public IApiService GetServiceByTaskId(Guid taskId)
    {
        return _taskServiceMap.TryGetValue(taskId, out var service) ? service : null;
    }
}

