using BusinessObject.DTO.Plan;
using Microsoft.AspNetCore.Mvc;
using Service.IService;

namespace HomeTrackAPI_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanPriceController : ControllerBase
    {
        private readonly IPlanPriceService _service;

        public PlanPriceController(IPlanPriceService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result); // service đảm bảo luôn trả [] nếu rỗng
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var planPrice = await _service.GetByIdAsync(id);
                return Ok(planPrice);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PlanPriceCreateUpdateDto dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
            
            //return CreatedAtAction(nameof(GetById), new { id = created.PlanPriceId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PlanPriceCreateUpdateDto dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

    }
}
