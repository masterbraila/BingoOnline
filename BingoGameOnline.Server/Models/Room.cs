using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BingoGameOnline.Server.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Name { get; set; }

        [Required]
        [StringLength(50)]
        public required string CreatorName { get; set; }

        // In the future, you can add navigation properties for players
    }
}
