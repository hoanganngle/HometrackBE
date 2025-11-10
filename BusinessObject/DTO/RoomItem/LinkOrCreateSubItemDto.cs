using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BusinessObject.DTO.RoomItem
{
    public class LinkOrCreateSubItemDto
    {
        [JsonIgnore]
        public Guid? SubItemId { get; set; }

        public string Name { get; set; }
        public int? SortOrder { get; set; }
        public string ImageUri { get; set; }

        // NEW
        public string? SubItemType { get; set; }

        public Guid? HouseId { get; set; }
        public Guid? FloorId { get; set; }

        public string? corlor {  get; set; }
        public string? description { get; set; }
    }

}
