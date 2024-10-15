using System.Collections.Concurrent;
using System.Net.WebSockets;
using ApiBank.Core.Interfaces;
using ApiBank.Infrastructure.Registries;
using ApiBank.Shared.DTOs;

namespace ApiBank.Infrastructure.Services;

public class CustomWebSocketManager  : ICustomWebSocketManager
{
     private readonly ConcurrentDictionary<Guid, WebSocketConnection> _connections = new();
        private readonly ILoggingService _loggingService;
        private readonly ServiceRegistry _serviceRegistry;

        public CustomWebSocketManager (ILoggingService loggingService, ServiceRegistry serviceRegistry)
        {
            _loggingService = loggingService;
            _serviceRegistry = serviceRegistry;

            _loggingService.LogAdded += OnLogAdded;
        }

        public async Task AddConnectionAsync(Guid taskId, WebSocket webSocket)
        {
            var connection = new WebSocketConnection
            {
                TaskId = taskId,
                WebSocket = webSocket,
                LastActivity = DateTime.UtcNow
            };
            _connections[taskId] = connection;

            await ReceiveMessagesAsync(connection);
        }

        private async Task ReceiveMessagesAsync(WebSocketConnection connection)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while (connection.WebSocket.State == WebSocketState.Open)
                {
                    var result = await connection.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await CloseConnectionAsync(connection.TaskId);
                        return;
                    }

                    connection.LastActivity = DateTime.UtcNow;
                }
            }
            catch (WebSocketException)
            {
                await CloseConnectionAsync(connection.TaskId);
            }
        }


        private async void OnLogAdded(object sender, LogEventArgs e)
        {
            if (_connections.TryGetValue(e.TaskId, out var connection))
            {
                if (connection.WebSocket.State == WebSocketState.Open)
                {
                    var logMessage = e.LogMessage;
                    var buffer = System.Text.Encoding.UTF8.GetBytes(logMessage);
                    await connection.WebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

                    connection.LastActivity = DateTime.UtcNow;
                }

                // if (connection.WebSocket.State == WebSocketMessageType.Close)
                // {
                //     await CloseConnectionAsync(e.TaskId);
                //
                // }
            }
        }

        public async Task CloseConnectionAsync(Guid taskId)
        {
            if (_connections.TryGetValue(taskId, out var connection))
            {
                var webSocket = connection.WebSocket;

                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived || webSocket.State == WebSocketState.CloseSent)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                }

                _connections.TryRemove(taskId, out _);
            }
        }


        public void CheckForInactiveConnections(TimeSpan timeout)
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in _connections)
            {
                var connection = kvp.Value;
                if (now - connection.LastActivity > timeout)
                {
                    _ = CloseConnectionAsync(connection.TaskId);
                }
            }
        }

        private class WebSocketConnection
        {
            public Guid TaskId { get; set; }
            public WebSocket WebSocket { get; set; }
            public DateTime LastActivity { get; set; }
        }
}
