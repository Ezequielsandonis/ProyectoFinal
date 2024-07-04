using Microsoft.AspNetCore.SignalR;

namespace TucConnect.Data
{
    public class ChatHub : Hub
    {
        // Método para enviar mensajes a todos los clientes conectados
        public async Task SendMessageToClients(string channelUrl, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", channelUrl, message);
        }
    }
}
