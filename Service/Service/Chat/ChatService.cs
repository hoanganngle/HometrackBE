using BusinessObject.DTO.Chat;
using BusinessObject.Models;
using Repo.IRepository;
using Service.IService.IChat;
using System.Text.Json;

namespace Service.Service.Chat
{
    public class ChatService : IChatService
    {
        private readonly IGeminiClient _gemini;
        private readonly IChatHistoryRepository _chatRepo;
        private readonly IChatSessionRepository _sessionRepo;
        private readonly IRoomPlacementRepository _placementRepo;

        private const string AssistantUserId = "ChatAI";

        public ChatService(
            IGeminiClient gemini,
            IChatHistoryRepository chatRepo,
            IChatSessionRepository sessionRepo,
            IRoomPlacementRepository placementRepo)
        {
            _gemini = gemini;
            _chatRepo = chatRepo;
            _sessionRepo = sessionRepo;
            _placementRepo = placementRepo;
        }

        public async Task<QuickChatResponse> ChatAsync(
            QuickChatRequest req,
            Guid? userId,
            CancellationToken ct)
        {
            var userIdStr = userId?.ToString() ?? "anonymous";

            // 1) Lấy / tạo session
            ChatSession session;
            if (req.SessionId.HasValue)
            {
                session = await _sessionRepo.GetAsync(req.SessionId.Value, ct)
                          ?? throw new InvalidOperationException("Session not found.");
            }
            else
            {
                session = new ChatSession
                {
                    UserId = userIdStr,
                    Title = TrimTitle(req.Content),
                };
                await _sessionRepo.CreateAsync(session, ct);
                await _sessionRepo.SaveChangesAsync(ct);
            }

            // 2) Thử tool-mode: sắp xếp đồ
            bool ranToolMode = false;
            string finalReply = string.Empty;
            string updatedSummary = string.Empty;

            try
            {
                (ranToolMode, finalReply, updatedSummary) =
                    await TryRunArrangeToolFlowAsync(userId, req, ct);
            }
            catch
            {
                ranToolMode = false;
            }

            // 3) Nếu không chạy tool-mode, fallback chat thường
            if (!ranToolMode)
            {
                finalReply = await _gemini.GenerateAsync(req.SystemPrompt, req.Content, ct);
                // một dòng, không xuống dòng
                finalReply = finalReply
                    .Replace("\r\n", " ")
                    .Replace("\n", " ")
                    .Replace("\r", " ")
                    .Trim();
            }

            // 4) Lưu lịch sử
            var userMsg = new ChatMessage
            {
                ChatSessionId = session.Id,
                UserId = userIdStr,
                Role = "user",
                Content = req.Content
            };
            await _chatRepo.AddAsync(userMsg, ct);

            var botMsg = new ChatMessage
            {
                ChatSessionId = session.Id,
                UserId = AssistantUserId,
                Role = "assistant",
                Content = (finalReply + updatedSummary).Trim()
            };
            await _chatRepo.AddAsync(botMsg, ct);

            session.UpdatedAt = DateTime.UtcNow;
            await _chatRepo.SaveChangesAsync(ct);
            await _sessionRepo.SaveChangesAsync(ct);

            return new QuickChatResponse((finalReply + updatedSummary).Trim(), session.Id);
        }

        // ===== Tool-mode: Arrange items =====

        private async Task<(bool handled, string reply, string updatedSummary)>
            TryRunArrangeToolFlowAsync(Guid? userId, QuickChatRequest req, CancellationToken ct)
        {
            if (userId is null || string.IsNullOrWhiteSpace(req.Content))
                return (false, "", "");

            // Nhận diện intent cơ bản
            var text = req.Content.ToLowerInvariant();
            if (!(text.Contains("sắp xếp") || text.Contains("bố trí") || text.Contains("dời") || text.Contains("arrange")))
                return (false, "", "");

            // Lấy room thực (đừng trả Guid.Empty trong repo)
            var roomId = await _placementRepo.GetDefaultRoomForUserAsync(userId.Value, ct);
            if (roomId == Guid.Empty)
                return (false, "Không xác định được phòng làm việc của bạn.", "");

            // Khai báo tool schemas (nếu client thật cần)
            var tools = new object[]
            {
                new {
                    name = "get_items_in_room",
                    description = "Lấy danh sách item trong phòng",
                    parameters = new {
                        type = "object",
                        properties = new { roomId = new { type = "string", format = "uuid" } },
                        required = new []{ "roomId" }
                    }
                },
                new {
                    name = "update_item_position",
                    description = "Cập nhật vị trí 1 item trong phòng",
                    parameters = new {
                        type = "object",
                        properties = new {
                            roomId     = new { type = "string", format = "uuid" },
                            roomItemId = new { type = "string", format = "uuid" },
                            x          = new { type = "number" },
                            y          = new { type = "number" }
                        },
                        required = new []{ "roomId", "roomItemId", "x", "y" }
                    }
                }
            };

            var systemPrompt = (req.SystemPrompt ?? "") + @"
Bạn là trợ lý sắp xếp đồ vật trong phòng.
- Khi cần xem đồ vật trong phòng, hãy gọi tool get_items_in_room(roomId).
- Khi muốn dời vị trí một đồ vật, hãy gọi tool update_item_position(roomId, roomItemId, x, y).
- Không tự tạo SQL. Mọi truy vấn/ghi DB sẽ do hệ thống thực thi.
- Tọa độ (x,y) là số thực (decimal).";

            // Bước 1: hỏi LLM muốn gọi tool gì
            LlmResponse resp;
            try
            {
                resp = await _gemini.RunWithToolsAsync(systemPrompt, req.Content, tools, ct);
            }
            catch (NotImplementedException)
            {
                return (false, "", "");
            }

            var updates = new List<RoomItemPositionDto>();

            // Orchestrate tối đa 5 bước
            for (int step = 0; step < 5; step++)
            {
                if (resp.ToolCalls == null || resp.ToolCalls.Count == 0)
                    break;

                foreach (var call in resp.ToolCalls)
                {
                    switch (call.Name)
                    {
                        case "get_items_in_room":
                            {
                                var items = await _placementRepo.GetPlacementsAsync(roomId, ct);
                                resp = await _gemini.ContinueWithToolResultAsync(systemPrompt, tools, call.Name, items, ct);
                                break;
                            }

                        case "update_item_position":
                            {
                                // Parse arguments an toàn
                                var raw = call.Arguments.GetRawText();
                                var args = JsonSerializer.Deserialize<UpdatePosArgs>(raw);
                                if (args == null
                                    || string.IsNullOrWhiteSpace(args.roomId)
                                    || string.IsNullOrWhiteSpace(args.roomItemId))
                                {
                                    resp = await _gemini.ContinueWithToolResultAsync(systemPrompt, tools, call.Name,
                                        new { ok = false, error = "invalid_args" }, ct);
                                    break;
                                }

                                Guid argRoomId = Guid.TryParse(args.roomId, out var g1) ? g1 : Guid.Empty;
                                Guid roomItemId = Guid.TryParse(args.roomItemId, out var g2) ? g2 : Guid.Empty;
                                decimal x = args.x;
                                decimal y = args.y;

                                // Dùng roomId thực nếu tool gửi Guid.Empty / sai phòng
                                var effectiveRoomId = argRoomId == Guid.Empty ? roomId : argRoomId;
                                if (roomItemId == Guid.Empty)
                                {
                                    resp = await _gemini.ContinueWithToolResultAsync(systemPrompt, tools, call.Name,
                                        new { ok = false, error = "invalid_roomItemId" }, ct);
                                    break;
                                }

                                // CẬP NHẬT DB
                                await _placementRepo.UpdatePositionAsync(effectiveRoomId, roomItemId, x, y, ct);
                                updates.Add(new RoomItemPositionDto { RoomItemId = roomItemId, X = x, Y = y });

                                // Thông báo lại cho LLM
                                resp = await _gemini.ContinueWithToolResultAsync(systemPrompt, tools, call.Name,
                                    new { ok = true, roomId = effectiveRoomId, roomItemId, x, y }, ct);
                                break;
                            }

                        default:
                            {
                                resp = await _gemini.ContinueWithToolResultAsync(systemPrompt, tools, call.Name,
                                    new { error = "unknown_tool" }, ct);
                                break;
                            }
                    }
                }
            }

            // Chuẩn hóa reply: một dòng
            var replyText = (resp.Message ?? "Đã sắp xếp xong.")
                .Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Trim();

            // Tóm tắt cập nhật: một dòng
            var updatedSummary = updates.Count == 0
                ? ""
                : " Đã cập nhật vị trí: " + string.Join(", ",
                    updates.Select(u => $"{u.RoomItemId}→({u.X},{u.Y})"));

            // Nếu repo bạn KHÔNG commit trong UpdatePositionAsync thì giữ dòng dưới;
            // nếu đã commit trong repo thì có thể bỏ.
            await _placementRepo.SaveChangesAsync(ct);

            return (true, replyText, updatedSummary);
        }

        private static string TrimTitle(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return "New chat";
            return content.Length <= 50 ? content : content[..50] + "...";
        }

        // Dùng để deserialize tool args
        private sealed class UpdatePosArgs
        {
            public string roomId { get; set; } = "";
            public string roomItemId { get; set; } = "";
            public decimal x { get; set; }
            public decimal y { get; set; }
        }
    }
}
