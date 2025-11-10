using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.RoomItem
{
    public class UpdateRoomItemRequest
    {
        public string Name { get; set; }
        public string SubName { get; set; }
        public string RoomType { get; set; }
        public decimal? X { get; set; }
        public decimal? Y { get; set; }
    }

}
