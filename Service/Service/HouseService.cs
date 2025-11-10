using BusinessObject.DTO.Floor;
using BusinessObject.DTO.House;
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
    public class HouseService : IHouseService
    {
        private readonly IHouseRepository _houseRepository;

        public HouseService(IHouseRepository houseRepository)
        {
            _houseRepository = houseRepository;
        }


        public async Task<IEnumerable<HouseResponseDto>> GetByUserIdAsync(Guid userId)
        {
            // Gọi repository đã Include đầy đủ Floors → Rooms → RoomItemInRooms → RoomItem
            var houses = await _houseRepository.GetByUserIdAsync(userId);

            if (houses == null || !houses.Any())
                return new List<HouseResponseDto>();

            return houses.Select(h => new HouseResponseDto
            {
                HouseId = h.HouseId,
                Name = h.Name,
                UserId = h.UserId,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt,
                image = h.Image,
                Floors = (h.Floors ?? Enumerable.Empty<Floor>())
                    .OrderBy(f => f.Level)
                    .Select(f => new FloorDto
                    {
                        FloorId = f.FloorId,
                        HouseId = f.HouseId,
                        Level = f.Level,
                        Name = f.Name,
                        CreatedAt = f.CreatedAt,
                        UpdatedAt = f.UpdatedAt,

                        Rooms = (f.Rooms ?? Enumerable.Empty<Room>())
                            .Select(r => new RoomDto
                            {
                                RoomId = r.RoomId,
                                Name = r.Name,
                                Type = r.Type,

                                Items = (r.RoomItemPlacements ?? Enumerable.Empty<RoomItemInRoom>())
                                    .Select(ri => new RoomItemInRoomDto
                                    {
                                        RoomItemId = ri.RoomItemId,
                                        RoomId = ri.RoomId,
                                        X = ri.X,
                                        Y = ri.Y,

                                        // Danh sách RoomItem con (nếu cần list)
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
                    }).ToList()
            }).ToList();
        }


        public async Task<HouseResponseDto?> GetByIdAsync(Guid id)
        {
            var house = await _houseRepository.GetByIdAsync(id);
            if (house == null) return null;

            return new HouseResponseDto
            {
                HouseId = house.HouseId,
                Name = house.Name,
                CreatedAt = house.CreatedAt,
                image = house.Image,
                Floors = house.Floors.Select(f => new FloorDto
                {
                    FloorId = f.FloorId,
                    Name = f.Name,
                    Level = f.Level,
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

                            // 🔹 Map danh sách RoomItem con (ví dụ: tất cả vật phẩm liên quan)
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
                }).ToList()
            };
        }

        public async Task<HouseResponseDto> CreateAsync(Guid userId, HouseRequestDto dto)
        {
            var now = DateTime.UtcNow;
            var house = new House
            {
                UserId = userId,
                Name = dto.Name,
                CreatedAt = now,
                Image = dto.image,
                UpdatedAt = null
            };

            await _houseRepository.AddAsync(house);
            await _houseRepository.SaveAsync();
            return MapToResponse(house);
        }

        public async Task<HouseResponseDto?> UpdateAsync(Guid id, HouseRequestDto dto)
        {
            var house = await _houseRepository.GetByIdAsync(id);
            if (house == null) return null;

            house.Name = dto.Name;
            house.UpdatedAt = DateTime.UtcNow;

            await _houseRepository.UpdateAsync(house);
            await _houseRepository.SaveAsync();

            return MapToResponse(house);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var house = await _houseRepository.GetByIdAsync(id);
            if (house == null) return false;

            await _houseRepository.DeleteAsync(house);
            await _houseRepository.SaveAsync();
            return true;
        }

        private HouseResponseDto MapToResponse(House house)
        {
            return new HouseResponseDto
            {
                HouseId = house.HouseId,
                UserId = house.UserId,
                Name = house.Name,
                CreatedAt = house.CreatedAt,
                UpdatedAt = house.UpdatedAt
            };
        }
    }
}
