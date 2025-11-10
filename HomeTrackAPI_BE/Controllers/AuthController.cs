using BusinessObject.DTO.Auth;
using BusinessObject.DTO.Email;
using BusinessObject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.IService;
using Service.Service;
using System.Security.Claims;

namespace HomeTrackAPI_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;
        private readonly JwtService _jwtService;
        private readonly IEmailService _emailService;
        public AuthController(IAuthService auth, JwtService jwt, IEmailService emailService)
        {
            _service = auth;
            _jwtService = jwt;
            _emailService = emailService;
        }

        [HttpGet("get-info-user")]
        [Authorize]
        public async Task<IActionResult> GetUserById()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            var userDto = await _service.GetUserByIdAsync(userId);
            return Ok(userDto);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _service.RegisterAsync(request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _service.LoginAsync(request);
            if (!result.Success) return Unauthorized(result);
            return Ok(result); 
        }

        [HttpPost("AddRole")]
        [Authorize]
        public async Task<IActionResult> AddRole([FromBody] CreateRoleRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RoleName))
                return BadRequest(new { success = false, error = "Role name là bắt buộc." });

            var role = new Role
            {
                RoleName = request.RoleName.Trim()
            };

            await _service.AddAsync(role); // không dùng CancellationToken

            return Ok(new
            {
                success = true,
                data = new { role.RoleName } // không trả roleId
            });
        }

        [HttpGet("role")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            return Ok(await _service.GetAllRolesAsync());
        }

        [HttpPost("check-otp-email")]
        public async Task<IActionResult> checkOtp([FromBody] CheckOtpRequest CheckOtpRequest)
        {
            try
            {
                bool isValidOtp = await _emailService.CheckOtpEmail(CheckOtpRequest);
                if (!isValidOtp)
                {
                    return BadRequest(new { message = "Otp không đúng. Vui lòng nhập lại" });
                }
                return Ok("Otp is valid!");
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("send-otp-email")]
        public async Task<IActionResult> SendOTPToEmail([FromBody] SendOtpEmailRequest sendOtpEmailRequest)
        {
            try
            {
                bool isSendOtp = await _emailService.SendEmailAsync(sendOtpEmailRequest);
                if (!isSendOtp)
                {
                    return BadRequest("Cannot send mail!");
                }
                return Ok("Send Otp successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{userId:guid}")]
        public async Task<IActionResult> ActiveUser(Guid userId, CancellationToken ct)
        {
            var ok = await _service.SetStatusAsync(userId, ct);
            if (!ok) return NotFound(new { message = "User not found" });
            return Ok("Cập nhập trạng thái tài khoản thành công!");
        }
    }
}
