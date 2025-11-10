using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.RoomItem
{
    public class RoomItemListItemDto
    {
        public Guid Id { get; set; }
        public string RoomType { get; set; }
        public string Name { get; set; }

        public string SubName { get; set; }

        public decimal? X { get; set; }
        public decimal? Y { get; set; }
    }

}
