using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BingoGameOnline.Server.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(int roomId, string user, string message)
        {
            await Clients.Group($"Room_{roomId}").SendAsync("ReceiveMessage", user, message);
        }
        public async Task JoinRoom(int roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{roomId}");
        }
        public async Task LeaveRoom(int roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Room_{roomId}");
        }
    }
}
