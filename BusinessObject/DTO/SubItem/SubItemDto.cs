using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.SubItem
{
    public class SubItemDto
    {
        public Guid SubItemId { get; set; }
        public Guid RoomItemId { get; set; }
        public string Name { get; set; }
        public string? ImageUri { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
