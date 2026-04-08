using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TeknikServis.Web.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public async Task JoinTenantGroup(string tenantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
    }

    public async Task JoinBranchGroup(string branchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"branch_{branchId}");
    }

    public async Task JoinUserGroup()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    public override async Task OnConnectedAsync()
    {
        await JoinUserGroup();
        await base.OnConnectedAsync();
    }
}
