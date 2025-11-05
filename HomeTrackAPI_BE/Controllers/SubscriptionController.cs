using BusinessObject.DTO.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.IService;
using System.Security.Claims;

namespace HomeTrackAPI_BE.Controllers
{
    [ApiController]
    [Route("api/Subcription")]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService _subService;
        public SubscriptionController(ISubscriptionService subService) => _subService = subService;

       

        [Authorize]
        [HttpPost("subcription-cancel")]
        public async Task<IActionResult> Cancel()
        {
            // Thử lấy từ các claim phổ biến
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("sub")
                     ?? User.FindFirst("user_id")
                     ?? User.FindFirst("uid");

            if (claim is null || !Guid.TryParse(claim.Value, out var userId))
                return Unauthorized(new { message = "Missing/invalid user id claim." });

            var result = await _subService.CancelByUserAsync(userId);
            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message, userId });
        }
    }
}
