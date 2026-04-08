using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TeknikServis.Web.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
    }

    public async Task SendTyping(string conversationId)
    {
        await Clients.OthersInGroup($"conv_{conversationId}")
            .SendAsync("UserTyping", Context.UserIdentifier, conversationId);
    }
}
