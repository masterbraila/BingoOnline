using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BingoGameOnline.Server.Models;

namespace BingoGameOnline.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Parameterless constructor for design-time tools
        public ApplicationDbContext() { }

        public DbSet<Room> Rooms { get; set; }
    }
}
