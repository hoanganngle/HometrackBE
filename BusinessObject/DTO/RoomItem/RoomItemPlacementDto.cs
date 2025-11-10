using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.RoomItem
{
    public class RoomItemPlacementDto
    {
        public Guid PlacementId { get; set; }
        public Guid RoomId { get; set; }
        public Guid ItemId { get; set; }
        public decimal? X { get; set; }
        public decimal? Y { get; set; }

        public string? ItemName { get; set; }
    }

}
