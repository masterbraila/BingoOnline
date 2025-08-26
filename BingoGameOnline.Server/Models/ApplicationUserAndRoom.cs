using Microsoft.AspNetCore.Identity;

namespace BingoGameOnline.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add additional properties if needed
    }

    public class Room
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? OwnerId { get; set; }
        public virtual ApplicationUser? Owner { get; set; }
        // Add more properties as needed (e.g., list of users, status, etc.)
    }
}
