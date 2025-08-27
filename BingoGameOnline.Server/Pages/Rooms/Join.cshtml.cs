using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BingoGameOnline.Server.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BingoGameOnline.Server.Pages.Rooms
{
    public class JoinModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public JoinModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public Room? Room { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public List<string> PlayerNames { get; set; } = new();
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Room = await _context.Rooms.FindAsync(id);
            if (Room == null)
            {
                return NotFound();
            }
            string playerName;
            bool isGuest;
            string? userId = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                playerName = User.Identity.Name ?? "User";
                isGuest = false;
                userId = User.Identity.Name;
            }
            else
            {
                if (HttpContext.Session.GetString("GuestName") is string guestName)
                {
                    playerName = guestName;
                }
                else
                {
                    guestName = $"Guest{new Random().Next(10000, 99999)}";
                    HttpContext.Session.SetString("GuestName", guestName);
                    playerName = guestName;
                }
                isGuest = true;
            }
            // Add player to room if not already present
            bool alreadyInRoom = await _context.RoomPlayers.AnyAsync(rp => rp.RoomId == id && rp.Name == playerName);
            if (!alreadyInRoom)
            {
                _context.RoomPlayers.Add(new RoomPlayer
                {
                    RoomId = id,
                    Name = playerName,
                    IsGuest = isGuest,
                    UserId = userId
                });
                await _context.SaveChangesAsync();
            }
            DisplayName = playerName;
            // Get all player names in the room
            PlayerNames = await _context.RoomPlayers
                .Where(rp => rp.RoomId == id)
                .Select(rp => rp.Name)
                .ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            string playerName;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                playerName = User.Identity.Name ?? "User";
            }
            else
            {
                playerName = HttpContext.Session.GetString("GuestName") ?? "Guest";
            }
            var player = await _context.RoomPlayers.FirstOrDefaultAsync(rp => rp.RoomId == id && rp.Name == playerName);
            if (player != null)
            {
                _context.RoomPlayers.Remove(player);
                await _context.SaveChangesAsync();
            }
            // If no players left in the room, delete the room
            bool anyPlayersLeft = await _context.RoomPlayers.AnyAsync(rp => rp.RoomId == id);
            if (!anyPlayersLeft)
            {
                var room = await _context.Rooms.FindAsync(id);
                if (room != null)
                {
                    _context.Rooms.Remove(room);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToPage("Index");
        }
    }
}
