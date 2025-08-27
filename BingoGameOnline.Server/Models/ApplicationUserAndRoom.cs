namespace BingoGameOnline.Server.Models
{
    public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser
    {
        // Add additional properties as needed
    }

    public class Room
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? OwnerId { get; set; }
        public ApplicationUser? Owner { get; set; }
        // Add more properties as needed
    }
}
