using BusinessObject.DTO.Floor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.House
{
    public class HouseResponseDto
    {
        public Guid HouseId { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? image {  get; set; }

        // NEW: kèm danh sách Floor của House
        public List<FloorDto> Floors { get; set; } = new();
    }
}
