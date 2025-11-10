using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.RoomItem
{
    public class UpsertRoomItemsRequest
    {
        public List<RoomItemUpsertDto> Items { get; set; } = new();

        public bool ReplaceAll { get; set; } = false;
    }


}
