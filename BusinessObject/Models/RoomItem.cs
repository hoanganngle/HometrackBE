using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    [Table("RoomItem")]
    public class RoomItem
    {
        [Key] public Guid RoomItemId { get; set; } = Guid.NewGuid();

        [Required, StringLength(100)]
        public string Item { get; set; } = default!;

        public string SubName { get; set; }
        // catalog metadata
        [StringLength(50)]
        public string? RoomType { get; set; } // ví dụ: Bedroom, Kitchen, ...

        public decimal? DefaultX { get; set; } // gợi ý mặc định khi add vào room
        public decimal? DefaultY { get; set; }

        public ICollection<SubItem> SubItems { get; set; }
        public ICollection<RoomItemInRoom> RoomPlacements { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
