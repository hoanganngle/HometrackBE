using BusinessObject.DTO.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.RoomItem
{
    public class RoomItemInRoomDto
    {
        public Guid RoomItemId { get; set; }
        public Guid RoomId { get; set; }
        public string? Name { get; set; }
        public string? Item { get; set; }
        public string? SubName { get; set; }
        public string? RoomType { get; set; }

        public decimal? X { get; set; }
        public decimal? Y { get; set; }

        public List<RoomItemDto> ListItem { get; set; } = new();
    }
}
