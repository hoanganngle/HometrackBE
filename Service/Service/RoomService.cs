using BusinessObject.DTO.Room;
using BusinessObject.DTO.RoomItem;
using BusinessObject.Models;
using Repo.IRepository;
using Service.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Service
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _repo;
        private static readonly string[] ValidRoomTypes =
            { "Bedroom", "LivingRoom", "Kitchen", "Bathroom", "Office", "Storage" };

        public RoomService(IRoomRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<RoomDto>> GetByFloorIdAsync(Guid floorId)
        {
            var rooms = await _repo.GetByFloorIdAsync(floorId);
            if (rooms == null || !rooms.Any())
                return new List<RoomDto>();

            return rooms.Select(r => new RoomDto
            {
                RoomId = r.RoomId,
                FloorId = r.FloorId,
                Name = r.Name,
                Type = r.Type,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,

                // ✅ Map danh sách vật phẩm trong phòng
                Items = (r.RoomItemPlacements ?? Enumerable.Empty<RoomItemInRoom>())
                    .Select(ri => new RoomItemInRoomDto
                    {
                        RoomId = ri.RoomId,
                        RoomItemId = ri.RoomItemId,
                        
                        X = ri.X,
                        Y = ri.Y,

                        // ✅ Danh sách RoomItem con
                        ListItem = ri.RoomItem == null
                            ? new List<RoomItemDto>()
                            : new List<RoomItemDto>
                            {
                        new RoomItemDto
                        {
                            RoomItemId = ri.RoomItem.RoomItemId,
                            Item = ri.RoomItem.Item,
                            SubName = ri.RoomItem.SubName,
                            RoomType = ri.RoomItem.RoomType,
                            DefaultX = ri.RoomItem.DefaultX,
                            DefaultY = ri.RoomItem.DefaultY
                        }
                            }
                    })
                    .ToList()
            })
            .ToList();
        }


        public async Task<RoomDto?> GetByIdAsync(Guid id)
        {
            var room = await _repo.GetByIdAsync(id);
            if (room == null)
                return null;

            return new RoomDto
            {
                RoomId = room.RoomId,
                FloorId = room.FloorId,
                Name = room.Name,
                Type = room.Type,
                CreatedAt = room.CreatedAt,
                UpdatedAt = room.UpdatedAt,

                // ✅ Map danh sách vật phẩm trong phòng
                Items = (room.RoomItemPlacements ?? Enumerable.Empty<RoomItemInRoom>())
                    .Select(ri => new RoomItemInRoomDto
                    {
                        RoomItemId = ri.RoomItemId,
                        RoomId = ri.RoomId,
                        
                        X = ri.X,
                        Y = ri.Y,

                        // ✅ Thêm danh sách RoomItem con (ListItem)
                        ListItem = ri.RoomItem == null
                            ? new List<RoomItemDto>()
                            : new List<RoomItemDto>
                            {
                        new RoomItemDto
                        {
                            RoomItemId = ri.RoomItem.RoomItemId,
                            Item = ri.RoomItem.Item,
                            SubName = ri.RoomItem.SubName,
                            RoomType = ri.RoomItem.RoomType,
                            DefaultX = ri.RoomItem.DefaultX,
                            DefaultY = ri.RoomItem.DefaultY
                        }
                            }
                    })
                    .ToList()
            };
        }


        public async Task<RoomDto?> AddAsync(AddRoomDto dto)
        {
            if (!ValidRoomTypes.Contains(dto.Type))
                throw new ArgumentException($"Invalid room type: {dto.Type}. Valid types: {string.Join(", ", ValidRoomTypes)}");

            var room = new Room
            {
                FloorId = dto.FloorId,
                Name = dto.Name,
                Type = dto.Type,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repo.AddAsync(room);
            return new RoomDto
            {
                RoomId = created.RoomId,
                FloorId = created.FloorId,
                Name = created.Name,
                Type = created.Type,
                CreatedAt = created.CreatedAt
            };
        }

        public async Task<RoomDto?> UpdateAsync(Guid id, UpdateRoomDto dto)
        {
            if (!ValidRoomTypes.Contains(dto.Type))
                throw new ArgumentException($"Invalid room type: {dto.Type}. Valid types: {string.Join(", ", ValidRoomTypes)}");

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            existing.Name = dto.Name;
            existing.Type = dto.Type;
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await _repo.UpdateAsync(existing);
            if (updated == null) return null;

            return new RoomDto
            {
                RoomId = updated.RoomId,
                FloorId = updated.FloorId,
                Name = updated.Name,
                Type = updated.Type,
                CreatedAt = updated.CreatedAt,
                UpdatedAt = updated.UpdatedAt
            };
        }

        public async Task<bool> DeleteAsync(Guid id) => await _repo.DeleteAsync(id);
    }
}
