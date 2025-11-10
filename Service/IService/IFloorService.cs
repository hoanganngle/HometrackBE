using BusinessObject.DTO.Floor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IService
{
    public interface IFloorService
    {
        Task<IEnumerable<FloorResponseDto>> GetByHouseIdAsync(Guid houseId);
        Task<FloorResponseDto?> GetByIdAsync(Guid id);
        Task<FloorResponseDto> CreateAsync(Guid houseId, FloorRequestDto dto);
        Task<FloorResponseDto?> UpdateAsync(Guid id, FloorRequestDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
