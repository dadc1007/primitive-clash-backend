using Microsoft.AspNetCore.SignalR;

namespace PrimitiveClash.WebAPI.Hubs;

/// <summary>
/// Hub de SignalR para tiempo real. Usa WebSockets cuando está disponible automáticamente.
/// </summary>
public class GameHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Une la conexión del cliente a un grupo de partida.
    /// </summary>
    public Task JoinMatch(string matchId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GroupName(matchId));
    }

    /// <summary>
    /// Sale del grupo de partida.
    /// </summary>
    public Task LeaveMatch(string matchId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(matchId));
    }

    /// <summary>
    /// Envía una acción al resto de clientes en la misma partida.
    /// </summary>
    public Task SendAction(string matchId, object action)
    {
        return Clients.Group(GroupName(matchId)).SendAsync("Action", new
        {
            Action = action,
            Sender = Context.ConnectionId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Ping simple para pruebas de conectividad.
    /// </summary>
    public Task<string> Ping() => Task.FromResult("pong");

    private static string GroupName(string matchId) => $"match:{matchId}";
}
