using Microsoft.AspNetCore.Identity;
using System;

namespace BingoGameOnline.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
