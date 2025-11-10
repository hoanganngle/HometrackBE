using BusinessObject.DTO.House;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IService
{
    public interface IHouseService
    {
        Task<IEnumerable<HouseResponseDto>> GetByUserIdAsync(Guid userId);
        Task<HouseResponseDto?> GetByIdAsync(Guid id);
        Task<HouseResponseDto> CreateAsync(Guid userId, HouseRequestDto dto);
        Task<HouseResponseDto?> UpdateAsync(Guid id, HouseRequestDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
