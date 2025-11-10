using BusinessObject.DTO.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.IService;
using System.Globalization;
using System.Security.Claims;

namespace HomeTrackAPI_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;

        public PaymentController(IPaymentService paymentService, IOrderService orderService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
        }


        [HttpPost]
        [Route("{userId:Guid}")]
        public async Task<IActionResult> SendPaymentLink(Guid userId, CreatePaymentRequest request)
        {
            try
            {
                var result = await _paymentService.SendPaymentLink(userId, request);
                if (result == null)
                {
                    return BadRequest(new { message = "Không thể tạo liên kết thanh toán." });
                }
                return Ok(result);
            }
            catch (BusinessException ex)
            {
                return StatusCode(ex.StatusCode, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { message = "Lỗi hệ thống. Vui lòng thử lại sau." });
            }
        }

        [AllowAnonymous]
        [HttpGet("paymentconfirm")]
        public async Task<IActionResult> PaymentConfirm()
        {
            const string FailUrl = "https://localhost:7227/api/Payment/payment-fail";
            //const string FailUrl = "https://hometrack.mlhr.org/api/Payment/payment-fail";

            if (!Request.Query.ContainsKey("orderCode") || !Request.Query.ContainsKey("orderId"))
                return Redirect(FailUrl);

            // 2) Parse an toàn (tránh FormatException & culture issues)
            if (!long.TryParse(Request.Query["orderCode"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var orderCode))
                return Redirect(FailUrl);

            if (!Guid.TryParse(Request.Query["orderId"], out var orderId) || orderId == Guid.Empty)
                return Redirect(FailUrl);

            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null) return Redirect(FailUrl);
                //if (order.OrderCode != orderCode) return Redirect(FailUrl);

                var amount = (decimal)order.AmountVnd;
                var accountId = order.UserId.ToString();
                var now = DateTime.UtcNow;

                DateTime existingTransactionDate = DateTime.Now;

                var queryRequest = new QueryRequest
                {
                    userId = accountId,
                    price = amount,
                    Paymentlink = orderCode.ToString(CultureInfo.InvariantCulture),
                    orderCode = orderCode, 
                    OrderId = orderId,
                    Url = Request.QueryString.Value!
                };

                var result = await _paymentService.ConfirmPayment(Request.QueryString.Value!, queryRequest);

                if (result != null && result.code == "00")
                {
                    string formattedAmount = string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} VND", amount);

                    string html = $@"
                        <!DOCTYPE html>
                        <html lang='vi'>
                        <head>
                          <meta charset='UTF-8'>
                          <title>Thanh toán thành công</title>
                          <meta name='viewport' content='width=device-width, initial-scale=1'>
                        </head>
                        <body style='text-align:center;font-family:sans-serif;padding:40px'>
                          <h1 style='color:green'>✅ BẠN ĐÃ THANH TOÁN THÀNH CÔNG</h1>
                          <p><strong>Mã đơn hàng (orderCode):</strong> {order.OrderCode}</p>
                          <p><strong>Số tiền:</strong> {formattedAmount}</p>
                          <p><strong>Ngày thanh toán (UTC):</strong> {now:dd/MM/yyyy HH:mm:ss}</p>
                          <hr/>
                          <p style='color:gray;'>Cảm ơn bạn đã sử dụng dịch vụ!</p>
                          <p style='margin-top:20px;font-size:16px;color:#333;'>Hãy quay lại ứng dụng để tiếp tục.</p>
                        </body>
                        </html>";
                    return Content(html, "text/html");
                }

                return Redirect(FailUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi xác nhận thanh toán: " + ex.Message);
                return Redirect(FailUrl);
            }
        }

        [HttpGet("payment-fail")]
        public IActionResult PaymentFail()
        {


            return Content($@"
                        <html>
                        <head>
                        <meta charset='UTF-8'>
                        <title>Thất bại</title>
                        </head>
                        <body style='text-align:center;font-family:sans-serif'>
                            <h1 style='color:red'>BẠN ĐÃ THANH TOÁN THẤT BẠI</h1>
                            <p>Giao dịch không thành công hoặc dữ liệu phản hồi không hợp lệ.</p>
                            <p>Xin vui lòng thử lại hoặc liên hệ hỗ trợ.</p>
                            <p style='margin-top:20px;font-size:16px;color:#333'>
                                Hãy quay lại ứng dụng của bạn để tiếp tục.
                            </p>
                        </body>
                        </html>", "text/html");


        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _paymentService.GetAllAsync();
            return Ok(transactions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var transaction = await _paymentService.GetByIdAsync(id);
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /*[HttpGet("user")]
        public async Task<IActionResult> GetByUser()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _paymentService.GetByUserIdAsync(userId);
            return Ok(result);
        }*/
        [HttpGet("status/{orderId:guid}")]
        public async Task<ActionResult<PaymentStatusDto>> GetStatusByOrderId(Guid orderId)
        {
            var dto = await _paymentService.GetStatusByOrderIdAsync(orderId);
            if (dto is null) return NotFound(new { message = "Không tìm thấy giao dịch cho OrderId." });
            return Ok(dto);
        }

        [HttpGet("GetPaymentByUserId")]
        public async Task<IActionResult> GetPaymentByUserId()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            if (userId == Guid.Empty)
                return Unauthorized();

            var result = await _paymentService.GetPaymentTransactionByUserIdAsync(userId);
            return Ok(result);
        }
    }
}
