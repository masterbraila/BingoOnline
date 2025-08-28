using BingoGameOnline.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BingoGameOnline.Server.Api
{
    [Route("api/friends")]
    [ApiController]
    [Authorize]
    public class FriendsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public FriendsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var friends = await _db.Friends
                .Where(f => f.UserId == userId)
                .Include(f => f.FriendUser)
                .Select(f => f.FriendUser.UserName)
                .ToListAsync();
            return Ok(friends);
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
                return BadRequest("Already friends");
            _db.Friends.Add(new Friend { UserId = userId, FriendUserId = friend.Id });
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
