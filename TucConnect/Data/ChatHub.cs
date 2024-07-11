

using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.AllExcept(Context.ConnectionId).SendAsync("ReceiveMessage", user, message);
    }

    public async Task AddToGroup(string channelUrl)
    {
        // Agrega al cliente actual al grupo identificado por el channelUrl
        await Groups.AddToGroupAsync(Context.ConnectionId, channelUrl);
    }

    public async Task RemoveFromGroup(string channelUrl)
    {
        // Remueve al cliente actual del grupo identificado por el channelUrl
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelUrl);
    }

    public async Task NotifyNewMessage(string channelUrl)
    {
        await Clients.All.SendAsync("NotifyNewMessage", channelUrl);
    }

    public async Task UpdateMessages(string channelUrl)
    {
        await Clients.All.SendAsync("UpdateMessages", channelUrl);
    }
}
