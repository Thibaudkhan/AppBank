using System.Text;
using ApiBank.Infrastructure.Registries;
using ApiBank.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ApiBank.Api.Controllers;

[ApiController]
[Route("api/v1/services")]
[Authorize] 
public class ServicesController: ControllerBase
{
    private readonly ServiceRegistry _serviceRegistry;

    public ServicesController(ServiceRegistry serviceRegistry)
    {
        _serviceRegistry = serviceRegistry;
    }

    // GET: /api/v1/services
    [HttpGet]
    public IActionResult GetAllServices()
    {
        var services = _serviceRegistry.GetAllServiceNames();
        return Ok(services);
    }

    
    [HttpGet("{serviceName}")]
    public IActionResult GetServiceActions(string serviceName)
    {
        try
        {
            var actions = _serviceRegistry.GetServiceActions(serviceName);
            return Ok(actions);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    [HttpPost("{serviceName}")]
    public async Task<IActionResult> ExecuteAction(string serviceName, [FromBody] ExecuteActionRequest request)
    {
        try
        {
            var service = _serviceRegistry.GetService(serviceName);
            var taskId = await service.ExecuteActionAsync(request.ActionName);
            return Accepted(new { TaskId = taskId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{serviceName}/logs/{taskId}")]
    public IActionResult GetLogs(string serviceName, Guid taskId)
    {
        try
        {
            var service = _serviceRegistry.GetService(serviceName);
            var logs = service.GetLogs(taskId);

            if (logs != null)
            {
                return Ok(logs);
            }
            else
            {
                return NotFound(new { message = "Logs non trouvés pour le TaskId spécifié." });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}