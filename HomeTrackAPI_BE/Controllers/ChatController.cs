using BusinessObject.DTO.Chat;
using BusinessObject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repo.IRepository;
using Service.IService.IChat;
using System.Security.Claims;

namespace HomeTrackAPI_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _svc;
        private readonly IChatSessionRepository _sessionRepo;
        private readonly IHttpContextAccessor _http;

        private const string AssistantUserId = "ChatAI";
        public ChatController(IChatService svc, IChatSessionRepository sessionRepo, IHttpContextAccessor http)
        {
            _svc = svc;
            _sessionRepo = sessionRepo;
            _http = http;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<QuickChatResponse>> Post(
            [FromBody] QuickChatRequest req,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Content))
                return BadRequest(new { error = "'content' is required" });

            // lấy userId từ claim như bạn đang làm ở GetUserById
            Guid? userId = null;
            var claimVal = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(claimVal, out var g))
                userId = g;

            // truyền xuống service
            var result = await _svc.ChatAsync(req, userId, ct);
            return Ok(result);
        }

        [HttpPost("{sessionId:guid}")]
        [Authorize]
        public async Task<ActionResult<QuickChatResponse>> PostToSession(
            Guid sessionId,
            [FromBody] QuickChatRequest req,
            CancellationToken ct)
        {
            // ép service dùng session này
            req.SessionId = sessionId;
            if (string.IsNullOrWhiteSpace(req.Content))
                return BadRequest(new { error = "'content' is required" });

            Guid? userId = null;
            var claimVal = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(claimVal, out var g))
                userId = g;

            // truyền xuống service
            var result = await _svc.ChatAsync(req, userId, ct);
            return Ok(result);
        }


        [HttpGet("sessions")]
        public async Task<ActionResult<List<ChatSession>>> GetSessions(CancellationToken ct)
        {
            var userId = _http.HttpContext?.User?.Identity?.Name ?? "user";
            var list = await _sessionRepo.ListByUserAsync(userId, ct);
            return Ok(list.Select(s => new {
                s.Id,
                s.Title,
                s.CreatedAt,
                s.UpdatedAt
            }));
        }

        [HttpGet("sessions/{id:guid}")]
        public async Task<ActionResult> GetSession(Guid id, CancellationToken ct)
        {
            var session = await _sessionRepo.GetAsync(id, ct);
            if (session == null) return NotFound();

            return Ok(new
            {
                session.Id,
                session.Title,
                session.CreatedAt,
                session.UpdatedAt,
                messages = session.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new {
                        m.Id,
                        m.Role,
                        m.Content,
                        m.CreatedAt
                    })
            });
        }
    }

}
