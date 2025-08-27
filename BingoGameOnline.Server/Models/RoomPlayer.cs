using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BingoGameOnline.Server.Models
{
    public class RoomPlayer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room Room { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public bool IsGuest { get; set; }

        public string? UserId { get; set; } // For registered users
    }
}
