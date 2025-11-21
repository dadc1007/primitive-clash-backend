using Microsoft.AspNetCore.SignalR;

namespace PrimitiveClash.Backend.Utils;

public static class HubExtensions
{
    public static Guid GetAuthenticatedUserId(this Hub hub)
    {
        string? oid = hub.Context.User?.FindFirst("oid")?.Value;
        
        if (string.IsNullOrWhiteSpace(oid) || !Guid.TryParse(oid, out Guid userId))
        {
            throw new HubException("Usuario no autenticado o token inválido");
        }

        return userId;
    }
    
    public static string GetAuthenticatedUserEmail(this Hub hub)
    {
        return hub.Context.User?.FindFirst("preferred_username")?.Value 
               ?? throw new HubException("Email no encontrado en token");
    }
}