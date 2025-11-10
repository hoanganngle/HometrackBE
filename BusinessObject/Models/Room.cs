using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    [Table("Room")]
    public class Room
    {
        [Key]
        public Guid RoomId { get; set; } = Guid.NewGuid();

        [Required, ForeignKey(nameof(Floor))]
        public Guid FloorId { get; set; }
        public Floor Floor { get; set; }

        [Required, StringLength(255)]
        public string Name { get; set; }

        [Required, StringLength(50)]
        public string Type { get; set; }

        // n–n với RoomItem qua bảng nối (đặt tên rõ nghĩa)
        public ICollection<RoomItemInRoom> RoomItemPlacements { get; set; } = new List<RoomItemInRoom>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

}
