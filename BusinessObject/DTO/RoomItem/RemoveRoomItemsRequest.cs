using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.RoomItem
{
    public class RemoveRoomItemsRequest
    {
        public List<Guid> ItemIds { get; set; } = new();
    }

}
