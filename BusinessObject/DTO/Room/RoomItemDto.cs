using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Room
{
    public class RoomItemDto
    {
        public Guid RoomItemId { get; set; }
        public string Item { get; set; }
        public string SubName { get; set; }
        public string RoomType { get; set; }

        public decimal? DefaultX { get; set; }
        public decimal? DefaultY { get; set; }
    }
}
