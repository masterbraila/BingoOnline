using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BingoGameOnline.Server.Hubs
{
    [Authorize]
    public class UserChatHub : Hub
    {
        // Send a message to a specific user
        public async Task SendPrivateMessage(string toUserId, string message)
        {
            var fromUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(fromUserId) && !string.IsNullOrEmpty(toUserId) && !string.IsNullOrWhiteSpace(message))
            {
                // Send to recipient
                await Clients.User(toUserId).SendAsync("ReceivePrivateMessage", fromUserId, toUserId, message);
                // Echo to sender
                await Clients.User(fromUserId).SendAsync("ReceivePrivateMessage", fromUserId, toUserId, message);
            }
        }
    }
}
