using BusinessObject.DTO.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Floor
{
    public class FloorResponseDto
    {
        public Guid FloorId { get; set; }
        public Guid HouseId { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string HouseName { get; set; }

        public List<RoomDto> Rooms { get; set; } = new();
    }
}
