using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class RoomItemInRoom
    {
        public Guid RoomId { get; set; }
        public Room Room { get; set; } = default!;
        public Guid RoomItemId { get; set; }
        public RoomItem RoomItem { get; set; } = default!;
        public decimal? X { get; set; } = 0;
        public decimal? Y { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // NEW: SubItems theo mỗi “bản đặt” trong phòng
        public ICollection<SubItem> SubItems { get; set; } = new List<SubItem>();
    }

}
