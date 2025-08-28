using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BingoGameOnline.Server.Models
{
    public class Friend
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string FriendUserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
        [ForeignKey("FriendUserId")]
        public ApplicationUser? FriendUser { get; set; }
    }
}
