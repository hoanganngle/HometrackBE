using BusinessObject.DTO.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.IService;
using System.Security.Claims;

namespace HomeTrackAPI_BE.Controllers
{
    [ApiController]
    [Route("api/orders")]
    
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService) => _orderService = orderService;

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null) return NotFound("Không tìm thấy đơn hàng!");

            return Ok(order);
        }
        [HttpGet("orderbyuserid")]
        public async Task<IActionResult> GetOrderByUserIdAsync()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId) || userId == Guid.Empty)
                return Unauthorized();

            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(orders); // nếu rỗng vẫn trả 200 cùng []
        }

        [HttpPost("createOrder")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> CreateUpgradeOrder([FromBody] CreateOrderRequest request)
        {
            var userIdStr = User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Không xác định được UserId.");

            var res = await _orderService.CreateUpgradeOrderAsync(userId, request);
            return Ok(res);
        }
    }
}
