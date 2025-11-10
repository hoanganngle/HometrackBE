using BusinessObject.DTO.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IService
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDto>> GetByFloorIdAsync(Guid floorId);
        Task<RoomDto?> GetByIdAsync(Guid id);
        Task<RoomDto?> AddAsync(AddRoomDto dto);
        Task<RoomDto?> UpdateAsync(Guid id, UpdateRoomDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
