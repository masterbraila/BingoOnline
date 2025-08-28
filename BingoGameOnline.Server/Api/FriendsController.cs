using BingoGameOnline.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BingoGameOnline.Server.Hubs;

namespace BingoGameOnline.Server.Api
{
    [Route("api/friends")]
    [ApiController]
    [Authorize]
    public class FriendsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<FriendsHub> _friendsHub;
        public FriendsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IHubContext<FriendsHub> friendsHub)
        {
            _db = db;
            _userManager = userManager;
            _friendsHub = friendsHub;
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            // Friends where I sent the request
            var sent = await (from f in _db.Friends
                              join u in _db.Users on f.FriendUserId equals u.Id
                              where f.UserId == userId && !string.IsNullOrEmpty(u.UserName)
                              select new {
                                  UserName = u.UserName,
                                  Status = f.Status,
                                  Direction = "sent",
                                  FriendId = f.Id,
                                  UserId = f.UserId,
                                  FriendUserId = f.FriendUserId
                              }).ToListAsync();
            // Friends where I received the request
            var received = await (from f in _db.Friends
                                  join u in _db.Users on f.UserId equals u.Id
                                  where f.FriendUserId == userId && !string.IsNullOrEmpty(u.UserName)
                                  select new {
                                      UserName = u.UserName,
                                      Status = f.Status,
                                      Direction = "received",
                                      FriendId = f.Id,
                                      UserId = f.UserId,
                                      FriendUserId = f.FriendUserId
                                  }).ToListAsync();
            return Ok(sent.Concat(received));
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromForm] string friendUsername)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(friendUsername))
                return BadRequest("Username required");
            var friend = await _userManager.FindByNameAsync(friendUsername);
            if (friend == null || friend.Id == userId)
                return BadRequest("User not found or cannot add yourself");
            bool alreadyFriends = await _db.Friends.AnyAsync(f => f.UserId == userId && f.FriendUserId == friend.Id);
            if (alreadyFriends)
                return BadRequest("Already friends or pending");
            _db.Friends.Add(new Friend { UserId = userId!, FriendUserId = friend.Id, Status = FriendStatus.Pending });
            await _db.SaveChangesAsync();
            // Notify the recipient
            await _friendsHub.Clients.User(friend.Id).SendAsync("FriendListChanged");
            return Ok();
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromForm] int friendId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var friend = await _db.Friends.FirstOrDefaultAsync(f => f.Id == friendId && f.FriendUserId == userId && f.Status == FriendStatus.Pending);
            if (friend == null)
                return NotFound();
            friend.Status = FriendStatus.Accepted;
            await _db.SaveChangesAsync();
            // Notify both users
            await _friendsHub.Clients.Users(new[] { friend.UserId, friend.FriendUserId }).SendAsync("FriendListChanged");
            return Ok();
        }

        [HttpPost("decline")]
        public async Task<IActionResult> Decline([FromForm] int friendId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var friend = await _db.Friends.FirstOrDefaultAsync(f => f.Id == friendId && f.FriendUserId == userId && f.Status == FriendStatus.Pending);
            if (friend == null)
                return NotFound();
            _db.Friends.Remove(friend);
            await _db.SaveChangesAsync();
            // Notify both users
            await _friendsHub.Clients.Users(new[] { friend.UserId, friend.FriendUserId }).SendAsync("FriendListChanged");
            return Ok();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Ok(new object[0]);
            var users = await _db.Users
                .Where(u => !string.IsNullOrEmpty(u.UserName) && u.UserName.Contains(q) && u.Id != userId)
                .OrderBy(u => u.UserName)
                .Select(u => new {
                    UserName = u.UserName,
                    UserId = u.Id
                })
                .Take(10)
                .ToListAsync();
            var myFriends = await _db.Friends
                .Where(f => (f.UserId == userId && users.Select(u => u.UserId).Contains(f.FriendUserId))
                         || (f.FriendUserId == userId && users.Select(u => u.UserId).Contains(f.UserId)))
                .ToListAsync();
            var result = users.Select(u => {
                var friend = myFriends.FirstOrDefault(f => (f.UserId == userId && f.FriendUserId == u.UserId) || (f.FriendUserId == userId && f.UserId == u.UserId));
                return new {
                    UserName = u.UserName,
                    UserId = u.UserId,
                    AlreadyFriend = friend != null,
                    Status = friend != null ? friend.Status.ToString().ToLower() : "none"
                };
            }).ToList();
            // DEBUG: Log the result
            System.Diagnostics.Debug.WriteLine(System.Text.Json.JsonSerializer.Serialize(result));
            return Ok(result);
        }
    }
}
