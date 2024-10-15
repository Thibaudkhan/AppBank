using ApiBank.Api;
using ApiBank.Application.Interfaces;
using ApiBank.Application.Services;
using ApiBank.Core.Interfaces;
using ApiBank.Infrastructure.ApiActions;
using ApiBank.Infrastructure.ApiServices;
using ApiBank.Infrastructure.Registries;
using ApiBank.Infrastructure.Services;
using Scrutor;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddControllers();

builder.Services.AddSingleton<IAuthService, AuthService>();

//builder.Services.AddSingleton<ITaskManager>(sp => new TaskManager(maxConcurrentTasks: 5));

//builder.Services.AddSingleton<ILoggingService, InMemoryLoggingService>();

//builder.Services.AddSingleton<WebSocketConnectionManager>();

//builder.Services.AddSingleton<WebSocketNotificationService>();

builder.Services.AddSingleton<ApiServiceA>();
builder.Services.AddSingleton<ApiServiceB>();

builder.Services.AddSingleton<IApiAction, Action1>();
builder.Services.AddSingleton<IApiAction, Action2>();
builder.Services.AddSingleton<IApiAction, Action3>();

builder.Services.AddSingleton<ServiceRegistry>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ILoggingService, InMemoryLoggingService>();
builder.Services.AddSingleton<WebSocketNotificationService>();

builder.Services.AddSingleton<ITaskManager, TaskManager>(provider =>
{
    var loggingService = provider.GetRequiredService<ILoggingService>();
    return new TaskManager(maxConcurrentTasks: 5);
});
builder.Services.AddSingleton<ICustomWebSocketManager, CustomWebSocketManager>();
//builder.Services.AddSingleton<ServiceRegistry>();

var app = builder.Build();

app.Services.GetRequiredService<WebSocketNotificationService>();

var serviceRegistry = app.Services.GetRequiredService<ServiceRegistry>();

var serviceA = app.Services.GetService<ApiServiceA>();
var serviceB = app.Services.GetService<ApiServiceB>();

if (serviceA != null) serviceRegistry.RegisterService(serviceA);
if (serviceB != null) serviceRegistry.RegisterService(serviceB);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120),
    ReceiveBufferSize = 4 * 1024
};
app.UseWebSockets(webSocketOptions);

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllers();

// Mapper le endpoint WebSocket
app.Map("/api/v1/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await HandleWebSocketConnection(context, webSocket);
    }
    else
    {
        context.Response.StatusCode = 400; // Mauvaise requête
    }
});

app.Run();

async Task HandleWebSocketConnection(HttpContext context, WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];

    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

    if (result.MessageType == WebSocketMessageType.Text)
    {
        var uuidString = Encoding.UTF8.GetString(buffer, 0, result.Count);
        if (Guid.TryParse(uuidString, out var taskId))
        {
            var connectionManager = context.RequestServices.GetRequiredService<WebSocketConnectionManager>();
            connectionManager.AddSocket(taskId, webSocket);

            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await connectionManager.RemoveSocket(taskId, webSocket);
                    }
                }
                catch (WebSocketException)
                {
                    await connectionManager.RemoveSocket(taskId, webSocket);
                }
            }
        }
        else
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "UUID invalide", CancellationToken.None);
        }
    }
    else
    {
        await webSocket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Message texte attendu contenant le TaskId", CancellationToken.None);
    }
}

public class WebSocketNotificationService
{
}
