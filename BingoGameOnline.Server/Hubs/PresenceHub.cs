using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BingoGameOnline.Server.Hubs
{
    [Authorize]
    public class PresenceHub : Hub
    {
        // userId -> connectionIds
        private static readonly ConcurrentDictionary<string, HashSet<string>> OnlineUsers = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                OnlineUsers.AddOrUpdate(userId,
                    _ => new HashSet<string> { Context.ConnectionId },
                    (_, set) => { set.Add(Context.ConnectionId); return set; });
                await Clients.All.SendAsync("UserOnline", userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                if (OnlineUsers.TryGetValue(userId, out var set))
                {
                    set.Remove(Context.ConnectionId);
                    if (set.Count == 0)
                    {
                        OnlineUsers.TryRemove(userId, out _);
                        await Clients.All.SendAsync("UserOffline", userId);
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public Task<List<string>> GetOnlineUsers()
        {
            return Task.FromResult(OnlineUsers.Keys.ToList());
        }
    }
}
