using BusinessObject.DTO.Floor;
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
    public class FloorService : IFloorService
    {
        private readonly IFloorRepository _floorRepository;

        public FloorService(IFloorRepository floorRepository)
        {
            _floorRepository = floorRepository;
        }

        public async Task<IEnumerable<FloorResponseDto>> GetByHouseIdAsync(Guid houseId)
        {
            var floors = await _floorRepository.GetByHouseIdAsync(houseId);

            if (floors == null || !floors.Any())
                return new List<FloorResponseDto>();

            return floors.Select(f => new FloorResponseDto
            {
                FloorId = f.FloorId,
                HouseId = f.HouseId,
                Level = f.Level,
                Name = f.Name,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt,
                HouseName = f.House?.Name,

                Rooms = f.Rooms.Select(r => new RoomDto
                {
                    RoomId = r.RoomId,
                    Name = r.Name,
                    Type = r.Type,

                    Items = r.RoomItemPlacements.Select(ri => new RoomItemInRoomDto
                    {
                        RoomItemId = ri.RoomItemId,
                        RoomId = ri.RoomId,
                        
                        X = ri.X,
                        Y = ri.Y,

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
                    }).ToList()
                }).ToList()
            })
            .ToList();
        }


        public async Task<FloorResponseDto?> GetByIdAsync(Guid id)
        {
            // Lấy dữ liệu từ repository (đã Include House, Rooms, RoomItemInRooms, RoomItem)
            var floor = await _floorRepository.GetByIdAsync(id);
            if (floor == null)
                return null;

            // Map sang DTO
            var dto = new FloorResponseDto
            {
                FloorId = floor.FloorId,
                HouseId = floor.HouseId,
                Level = floor.Level,
                Name = floor.Name,
                CreatedAt = floor.CreatedAt,
                UpdatedAt = floor.UpdatedAt,
                HouseName = floor.House?.Name,
                Rooms = floor.Rooms.Select(r => new RoomDto
                {
                    RoomId = r.RoomId,
                    Name = r.Name,
                    Type = r.Type,

                    // Map danh sách vật phẩm trong Room
                    Items = r.RoomItemPlacements.Select(ri => new RoomItemInRoomDto
                    {
                        RoomItemId = ri.RoomItemId,
                        RoomId = ri.RoomId,
                        X = ri.X,
                        Y = ri.Y,
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
                    }).ToList()
                }).ToList()
            };

            return dto;
        }


        public async Task<FloorResponseDto> CreateAsync(Guid houseId, FloorRequestDto dto)
        {
            var now = DateTime.UtcNow;
            var floor = new Floor
            {
                HouseId = houseId,
                Level = dto.Level,
                Name = dto.Name,
                CreatedAt = now,
                UpdatedAt = null // ✅ để null khi mới tạo
            };

            await _floorRepository.AddAsync(floor);
            await _floorRepository.SaveAsync();

            return MapToResponse(floor);
        }

        public async Task<FloorResponseDto?> UpdateAsync(Guid id, FloorRequestDto dto)
        {
            var floor = await _floorRepository.GetByIdAsync(id);
            if (floor == null) return null;

            floor.Level = dto.Level;
            floor.Name = dto.Name;
            floor.UpdatedAt = DateTime.UtcNow;

            await _floorRepository.UpdateAsync(floor);
            await _floorRepository.SaveAsync();

            return MapToResponse(floor);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var floor = await _floorRepository.GetByIdAsync(id);
            if (floor == null) return false;

            await _floorRepository.DeleteAsync(floor);
            await _floorRepository.SaveAsync();
            return true;
        }

        private FloorResponseDto MapToResponse(Floor floor)
        {
            return new FloorResponseDto
            {
                FloorId = floor.FloorId,
                HouseId = floor.HouseId,
                Level = floor.Level,
                Name = floor.Name,
                CreatedAt = floor.CreatedAt,
                UpdatedAt = floor.UpdatedAt
            };
        }
    }
}
