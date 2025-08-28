using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BingoGameOnline.Server.Hubs
{
    [Authorize]
    public class FriendsHub : Hub
    {
        // Notify a user (by userId) to reload their friends list
        public async Task NotifyFriendListChanged(string userId)
        {
            await Clients.User(userId).SendAsync("FriendListChanged");
        }
        // Optionally: allow server to call this for multiple users
        public static async Task NotifyUsers(IHubContext<FriendsHub> hub, params string[] userIds)
        {
            await hub.Clients.Users(userIds).SendAsync("FriendListChanged");
        }
    }
}
