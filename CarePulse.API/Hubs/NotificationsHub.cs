using Microsoft.AspNetCore.SignalR;

namespace CarePulse.API.Hubs;

public class NotificationsHub : Hub
{
    // Intentionally minimal hub: server will push appointment updates to connected clients.
}
