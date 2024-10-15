using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace ApiBank.Infrastructure.Services;

public class WebSocketConnectionManager
{
    private readonly ConcurrentDictionary<Guid, ConcurrentBag<WebSocket>> _socketsByTaskId = new ConcurrentDictionary<Guid, ConcurrentBag<WebSocket>>();

    public void AddSocket(Guid taskId, WebSocket socket)
    {
        var sockets = _socketsByTaskId.GetOrAdd(taskId, _ => new ConcurrentBag<WebSocket>());
        sockets.Add(socket);
    }

    public async Task RemoveSocket(Guid taskId, WebSocket socket)
    {
        if (_socketsByTaskId.TryGetValue(taskId, out var sockets))
        {
            var newSockets = new ConcurrentBag<WebSocket>(sockets.Except(new[] { socket }));
            if (newSockets.IsEmpty)
            {
                _socketsByTaskId.TryRemove(taskId, out _);
            }
            else
            {
                _socketsByTaskId[taskId] = newSockets;
            }
        }

        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connexion fermée", CancellationToken.None);
    }

    public IEnumerable<WebSocket> GetSockets(Guid taskId)
    {
        if (_socketsByTaskId.TryGetValue(taskId, out var sockets))
        {
            return sockets;
        }
        return Enumerable.Empty<WebSocket>();
    }
}
