using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Chat
{
    public class RoomItemPositionDto
    {
        public Guid RoomItemId { get; set; }
        public decimal X { get; set; }
        public decimal Y { get; set; }
    }
}
