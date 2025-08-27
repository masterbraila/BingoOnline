using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BingoGameOnline.Server.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BingoGameOnline.Server.Pages.Rooms
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }
        [BindProperty]
        public required Room Room { get; set; }
        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            // Assign next available room name
            int nextRoomNumber = (await _context.Rooms.CountAsync()) + 1;
            Room.Name = $"Room_{nextRoomNumber}";
            // Set creator name
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                Room.CreatorName = User.Identity.Name ?? "User";
            }
            else
            {
                var random = new Random();
                Room.CreatorName = $"Guest{random.Next(10000, 99999)}";
            }
            _context.Rooms.Add(Room);
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
