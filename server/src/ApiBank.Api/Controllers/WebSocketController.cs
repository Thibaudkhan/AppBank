using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Text;
using ApiBank.Core.Interfaces;
using ApiBank.Infrastructure.Registries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ApiBank.Api.Controllers;

[ApiController]
[Route("api/v1/ws")]
public class WebSocketController : ControllerBase
{
    private readonly ServiceRegistry _serviceRegistry;
    private readonly IConfiguration _configuration;
    private readonly ICustomWebSocketManager _webSocketManager;

    public WebSocketController(IConfiguration configuration,ServiceRegistry serviceRegistry,ICustomWebSocketManager webSocketManager)
    {
        _serviceRegistry = serviceRegistry;
        _configuration = configuration;
        _webSocketManager = webSocketManager;


    }
    [HttpGet("{taskId}")]
    public async Task<IActionResult> Get(Guid taskId, [FromQuery] string token)
    {
        if (HttpContext.Response.HasStarted)
        {
            return StatusCode(500, "La réponse a déjà été envoyée.");
        }

        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await _webSocketManager.AddConnectionAsync(taskId, webSocket);

            while (webSocket.State == WebSocketState.Open)
            {
                await Task.Delay(1000);  // Simuler la réception de logs ou d'autres événements
            }

            return Ok();
        }
        else
        {
            return BadRequest("La requête n'est pas une requête WebSocket.");
        }
    }



    // [HttpGet("{taskId}")]
    // public async Task GetLogsOverWebSocket(Guid taskId, [FromQuery] string token)
    // {
    //     if (HttpContext.WebSockets.IsWebSocketRequest)
    //     {
    //         if (ValidateToken(token))
    //         {
    //             using (var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
    //             {
    //                 await SendLogsToClientAsync(webSocket, taskId);
    //             }
    //         }
    //         else
    //         {
    //             HttpContext.Response.StatusCode = 401; 
    //         }
    //     }
    //     else
    //     {
    //         HttpContext.Response.StatusCode = 400;
    //     }
    // }

    private bool IsValidToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task SendLogsToClientAsync(WebSocket webSocket, Guid taskId)
    {
        var buffer = new byte[1024 * 4];
        var cancellationToken = HttpContext.RequestAborted;

        var service = _serviceRegistry.GetServiceByTaskId(taskId); 

        if (service == null)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Service non trouvé pour le TaskId fourni", cancellationToken);
            return;
        }

        string logs;
        do
        {
            logs = service.GetLogs(taskId); 

            if (!string.IsNullOrEmpty(logs))
            {
                var logBytes = Encoding.UTF8.GetBytes(logs);
                await webSocket.SendAsync(new ArraySegment<byte>(logBytes, 0, logBytes.Length), WebSocketMessageType.Text, true, cancellationToken);
            }

            await Task.Delay(1000);  
        } while (!webSocket.CloseStatus.HasValue && logs != null);

        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Fin des logs", cancellationToken);
    }

}