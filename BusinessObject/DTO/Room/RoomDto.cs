using BusinessObject.DTO.RoomItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Room
{
    public class RoomDto
    {
        public Guid RoomId { get; set; }
        public Guid FloorId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<RoomItemInRoomDto> Items { get; set; } = new();
    }
}
