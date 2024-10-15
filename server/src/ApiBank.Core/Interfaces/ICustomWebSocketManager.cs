using System.Net.WebSockets;

namespace ApiBank.Core.Interfaces;

public interface ICustomWebSocketManager
{
    Task AddConnectionAsync(Guid taskId, WebSocket webSocket);
    Task CloseConnectionAsync(Guid taskId);
    void CheckForInactiveConnections(TimeSpan timeout);
}