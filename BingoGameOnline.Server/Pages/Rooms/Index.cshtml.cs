using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using BingoGameOnline.Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using BingoGameOnline.Server.Hubs;

namespace BingoGameOnline.Server.Pages.Rooms
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoomHubNotifier _notifier;
        public IndexModel(ApplicationDbContext context, IRoomHubNotifier notifier)
        {
            _context = context;
            _notifier = notifier;
        }
        public IList<Room> Rooms { get; set; } = new List<Room>();
        private List<IGrouping<int, RoomPlayer>> _roomPlayers = new();
        public async Task OnGetAsync()
        {
            Rooms = await _context.Rooms.ToListAsync();
            _roomPlayers = await _context.RoomPlayers.GroupBy(rp => rp.RoomId).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            int nextRoomNumber = (await _context.Rooms.CountAsync()) + 1;
            string creatorName;
            bool isGuest;
            string? userId = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                creatorName = User.Identity.Name ?? "User";
                isGuest = false;
                userId = User.Identity.Name;
            }
            else
            {
                if (HttpContext.Session.GetString("GuestName") is string guestName)
                {
                    creatorName = guestName;
                }
                else
                {
                    guestName = $"Guest{new Random().Next(10000, 99999)}";
                    HttpContext.Session.SetString("GuestName", guestName);
                    creatorName = guestName;
                }
                isGuest = true;
            }
            var room = new Room
            {
                Name = $"Room_{nextRoomNumber}",
                CreatorName = creatorName
            };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            // Auto-join creator to the room
            _context.RoomPlayers.Add(new RoomPlayer
            {
                RoomId = room.Id,
                Name = creatorName,
                IsGuest = isGuest,
                UserId = userId
            });
            await _context.SaveChangesAsync();
            // Do NOT notify here; redirect to Join, and let Join/Leave handle notifications
            return RedirectToPage("Join", new { id = room.Id });
        }
        public int GetPlayerCount(int roomId)
        {
            return _roomPlayers.FirstOrDefault(g => g.Key == roomId)?.Count() ?? 0;
        }
    }
}
