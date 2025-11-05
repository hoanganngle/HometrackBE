using BusinessObject.DTO.Plan;
using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IService
{
    public interface IPlanPriceService
    {
        Task<IEnumerable<PlanPriceDto>> GetAllAsync();
        Task<PlanPriceDto?> GetByIdAsync(Guid id);
        Task<PlanPriceDto> CreateAsync(PlanPriceCreateUpdateDto dto);
        Task<PlanPriceDto?> UpdateAsync(Guid id, PlanPriceCreateUpdateDto dto);
    }
}
