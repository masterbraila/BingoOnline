using BingoGameOnline.Server.Data;
using BingoGameOnline.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BingoGameOnline.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoomController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();
            var room = new Room { Name = model.Name, OwnerId = userId };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return Ok(room);
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListRooms()
        {
            var rooms = await _context.Rooms.Include(r => r.Owner).ToListAsync();
            return Ok(rooms);
        }

        [HttpPost("join/{roomId}")]
        public async Task<IActionResult> JoinRoom(int roomId)
        {
            // Implement join logic as needed (e.g., add user to a room's user list)
            // For now, just check if room exists
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
                return NotFound();
            return Ok();
        }

        [HttpDelete("delete/{roomId}")]
        public async Task<IActionResult> DeleteRoom(int roomId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
                return NotFound();
            if (room.OwnerId != userId)
                return Forbid();
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public class CreateRoomModel
    {
        public string? Name { get; set; }
    }
}
