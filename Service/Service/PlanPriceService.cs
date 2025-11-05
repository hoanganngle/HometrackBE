using BusinessObject.DTO.Plan;
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
    public class PlanPriceService : IPlanPriceService
    {
        private readonly IPlanPriceRepository _repository;

        public PlanPriceService(IPlanPriceRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PlanPriceDto>> GetAllAsync()
        {
            var data = await _repository.GetAllAsync();

            // Nếu không có dữ liệu → trả mảng rỗng []
            if (data == null || !data.Any())
                return new List<PlanPriceDto>();

            return data.Select(p => new PlanPriceDto
            {
                PlanPriceId = p.PlanPriceId,
                PlanId = p.PlanId,
                PlanName = p.Plan?.Name,
                Period = p.Period.ToString(),
                DurationInDays = p.DurationInDays,
                AmountVnd = p.AmountVnd,
                IsActive = p.IsActive,
                CreateAt = p.CreateAt,
                UpdateAt = p.UpdateAt
            });
        }

        public async Task<PlanPriceDto?> GetByIdAsync(Guid id)
        {
            var p = await _repository.GetByIdAsync(id);
            if (p == null)
                throw new Exception($"PlanPrice with ID {id} not found.");

            return new PlanPriceDto
            {
                PlanPriceId = p.PlanPriceId,
                PlanId = p.PlanId,
                PlanName = p.Plan?.Name,
                Period = p.Period.ToString(),
                DurationInDays = p.DurationInDays,
                AmountVnd = p.AmountVnd,
                IsActive = p.IsActive,
                CreateAt = p.CreateAt,
                UpdateAt = p.UpdateAt
            };
        }

        public async Task<PlanPriceDto> CreateAsync(PlanPriceCreateUpdateDto dto)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);


            var entity = new PlanPrice
            {
                //PlanPriceId = Guid.NewGuid(),
                PlanId = dto.PlanId,
                Period = dto.Period,
                DurationInDays = dto.DurationInDays,
                AmountVnd = dto.AmountVnd,
                IsActive = dto.IsActive,
                CreateAt = vietnamNow
            };

            await _repository.AddAsync(entity);

            return new PlanPriceDto
            {
                //PlanPriceId = entity.PlanPriceId,
                PlanId = entity.PlanId,
                Period = entity.Period.ToString(),
                DurationInDays = entity.DurationInDays,
                AmountVnd = entity.AmountVnd,
                IsActive = entity.IsActive,
                CreateAt = entity.CreateAt
            };
        }

        public async Task<PlanPriceDto?> UpdateAsync(Guid id, PlanPriceCreateUpdateDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) 
                throw new Exception($"PlanPrice with ID {id} not found.");

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);


            existing.PlanId = dto.PlanId;
            existing.Period = dto.Period;
            existing.DurationInDays = dto.DurationInDays;
            existing.AmountVnd = dto.AmountVnd;
            existing.IsActive = dto.IsActive;
            existing.UpdateAt = vietnamNow;

            await _repository.UpdateAsync(existing);

            return new PlanPriceDto
            {
                //PlanPriceId = existing.PlanPriceId,
                PlanId = existing.PlanId,
                PlanName = existing.Plan?.Name,
                Period = existing.Period.ToString(),
                DurationInDays = existing.DurationInDays,
                AmountVnd = existing.AmountVnd,
                IsActive = existing.IsActive,
                UpdateAt = existing.UpdateAt
            };
        }


    }
}
