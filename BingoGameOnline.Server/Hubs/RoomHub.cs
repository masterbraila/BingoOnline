using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BingoGameOnline.Server.Hubs
{
    public class RoomHub : Hub
    {
        public async Task JoinRoomGroup(int roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{roomId}");
        }
        public async Task LeaveRoomGroup(int roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Room_{roomId}");
        }
        public async Task NotifyRoomChanged()
        {
            await Clients.All.SendAsync("RoomListChanged");
        }
        public async Task NotifyPlayerListChanged(int roomId)
        {
            await Clients.Group($"Room_{roomId}").SendAsync("PlayerListChanged");
        }
    }
}
