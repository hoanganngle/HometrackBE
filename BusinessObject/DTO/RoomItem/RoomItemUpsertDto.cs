using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.RoomItem
{
    public class RoomItemUpsertDto
    {
        // ID của item gốc (bảng RoomItem)
        public Guid ItemId { get; set; }

        // Toạ độ muốn đặt trong Room
        public decimal? X { get; set; }
        public decimal? Y { get; set; }
    }
}
