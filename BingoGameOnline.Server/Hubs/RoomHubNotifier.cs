using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BingoGameOnline.Server.Hubs
{
    public interface IRoomHubNotifier
    {
        Task NotifyRoomChanged();
        Task NotifyPlayerListChanged(int roomId);
    }

    public class RoomHubNotifier : IRoomHubNotifier
    {
        private readonly IHubContext<RoomHub> _hubContext;
        public RoomHubNotifier(IHubContext<RoomHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task NotifyRoomChanged()
        {
            await _hubContext.Clients.All.SendAsync("RoomListChanged");
        }
        public async Task NotifyPlayerListChanged(int roomId)
        {
            await _hubContext.Clients.Group($"Room_{roomId}").SendAsync("PlayerListChanged");
        }
    }
}
