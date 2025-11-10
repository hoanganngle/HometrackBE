using BusinessObject.DTO.Chat;
using BusinessObject.DTO.RoomItem;
using Microsoft.Extensions.Options;
using Service.IService.IChat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service.Service.Chat
{
    public class GeminiClient : IGeminiClient
    {
        private readonly HttpClient _http;
        private readonly GeminiOptions _opt;
        private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        private string? _lastUserMessage = string.Empty;

        public GeminiClient(IHttpClientFactory f, IOptions<GeminiOptions> opt)
        {
            _http = f.CreateClient();
            _opt = opt.Value;
        }

        public async Task<string> GenerateAsync(string? systemPrompt, string userText, CancellationToken ct)
        {
            var apiKey = !string.IsNullOrWhiteSpace(_opt.ApiKey)
                ? _opt.ApiKey
                : Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Gemini API key is missing. Set Gemini:ApiKey or env GEMINI_API_KEY.");

            // 1) Gộp system + user
            var combined = string.IsNullOrWhiteSpace(systemPrompt)
                ? userText
                : $"{systemPrompt}\n\n---\n\n{userText}";

            // 2) Làm sạch base endpoint
            string Clean(string? s)
            {
                var b = (s ?? "https://generativelanguage.googleapis.com/v1").TrimEnd('/');
                if (b.EndsWith("/models", StringComparison.OrdinalIgnoreCase)) b = b[..^7];
                return b;
            }

            var baseV1 = Clean(_opt.Endpoint);
            var baseV1beta = baseV1.EndsWith("/v1", StringComparison.OrdinalIgnoreCase)
                ? baseV1[..^2] + "1beta"
                : "https://generativelanguage.googleapis.com/v1beta";

            // 3) URL thử theo thứ tự: v1 -> v1beta
            var urls = new[]
            {
        $"{baseV1}/models/{_opt.Model}:generateContent?key={apiKey}",
        $"{baseV1beta}/models/{_opt.Model}:generateContent?key={apiKey}"
    };

            // 4) Payload
            var payload = new
            {
                contents = new[]
                {
            new { role = "user", parts = new[] { new { text = combined } } }
        }
            };
            var payloadJson = JsonSerializer.Serialize(payload, _json);

            // 5) Thử gửi
            foreach (var tryUrl in urls)
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, tryUrl)
                {
                    Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
                };

                using var res = await _http.SendAsync(req, ct);
                var body = await res.Content.ReadAsStringAsync(ct);

                if (res.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(body);
                    if (!doc.RootElement.TryGetProperty("candidates", out var cands) || cands.GetArrayLength() == 0)
                        return string.Empty;

                    /*var sb = new StringBuilder();
                    var parts = cands[0].GetProperty("content").GetProperty("parts");
                    foreach (var p in parts.EnumerateArray())
                        if (p.TryGetProperty("text", out var t)) sb.Append(t.GetString());
                    return sb.ToString();*/

                    var sb = new StringBuilder();
                    var parts = cands[0].GetProperty("content").GetProperty("parts");
                    foreach (var p in parts.EnumerateArray())
                    {
                        if (p.TryGetProperty("text", out var t))
                        {
                            var piece = t.GetString() ?? string.Empty;
                            sb.Append(
                                piece
                                    .Replace("\r\n", " ")
                                    .Replace("\n", " ")
                                    .Replace("\r", " ")
                            );
                        }
                    }
                    // Xóa khoảng trắng thừa và cắt đầu/cuối
                    var oneLine = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
                    return oneLine;

                }

                // Nếu là 404 -> thử URL kế tiếp (v1beta). Lỗi khác -> ném ngay kèm URL và body để debug
                if ((int)res.StatusCode != 404)
                    throw new InvalidOperationException($"Gemini error {(int)res.StatusCode} (url={tryUrl}): {body}");

                // Console.WriteLine để bạn nhìn rõ
                Console.WriteLine($"[Gemini WARN] 404 on {tryUrl}: {body}");
            }

            // Cả v1 và v1beta đều 404
            throw new InvalidOperationException(
                $"Gemini 404 on both v1 and v1beta for model '{_opt.Model}'. " +
                "Please list your available models with /diag/gemini-models and set Gemini:Model accordingly.");
        }

        public Task<LlmResponse> RunWithToolsAsync(
        string systemPrompt,
        string userMessage,
        IEnumerable<object> toolSchemas,
        CancellationToken ct = default)
        {
            _lastUserMessage = userMessage ?? string.Empty;

            var text = _lastUserMessage.ToLowerInvariant();
            var wantArrange = text.Contains("sắp xếp") || text.Contains("bố trí") || text.Contains("dời") || text.Contains("arrange");
            if (!wantArrange)
            {
                return Task.FromResult(new LlmResponse
                {
                    Message = "(mock) Bạn muốn làm gì? Hãy nói 'sắp xếp' để mình hỗ trợ.",
                    ToolCalls = new List<LlmToolCall>()
                });
            }

            var args = JsonDocument.Parse("{\"roomId\":\"00000000-0000-0000-0000-000000000000\"}").RootElement;
            return Task.FromResult(new LlmResponse
            {
                Message = null,
                ToolCalls = new List<LlmToolCall> {
                new LlmToolCall { Name = "get_items_in_room", Arguments = args }
            }
            });
        }

        public Task<LlmResponse> ContinueWithToolResultAsync(
    string systemPrompt,
    IEnumerable<object> toolSchemas,
    string toolName,
    object toolResult,
    CancellationToken ct = default)
        {
            if (toolName == "get_items_in_room")
            {
                var items = JsonSerializer.Deserialize<List<RoomItemInRoomDto>>(
                    JsonSerializer.Serialize(toolResult)) ?? new();

                string Normalize(string s) => s.ToLowerInvariant()
                    .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
                    .Replace("ắ", "a").Replace("ằ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
                    .Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
                    .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
                    .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
                    .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
                    .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
                    .Replace("ý", "y").Replace("ỳ", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y")
                    .Replace("đ", "d");

                var query = Normalize(_lastUserMessage ?? string.Empty);

                // Ưu tiên match đúng tên trong câu; sau đó “máy ảnh/camera”; rồi fallback item đầu.
                var target = items.FirstOrDefault(i =>
                {
                    var n = Normalize(i.SubName ?? i.Name ?? "");
                    return n.Length > 0 && query.Contains(n);
                })
                ?? items.FirstOrDefault(i =>
                {
                    var n = Normalize(i.SubName ?? i.Name ?? "");
                    return n.Contains("may anh") || n.Contains("camera");
                })
                ?? items.FirstOrDefault();

                if (target == null)
                {
                    return Task.FromResult(new LlmResponse
                    {
                        Message = "(mock) Không tìm thấy đồ vật để sắp xếp.",
                        ToolCalls = new List<LlmToolCall>()
                    });
                }

                // Tính vị trí mới theo grid, tránh trùng toạ độ với vật khác.
                var others = items.Where(i => i.RoomItemId != target.RoomItemId).ToList();

                (decimal newX, decimal newY) = PickNonOverlappingPosition(
                    preferredX: (target.X ?? 0m) + 20m,   // nudge nhẹ sang phải nếu đã có X
                    preferredY: target.Y ?? 40m,         // nếu rỗng thì bắt đầu 40
                    existing: others,
                    step: 40m,                      // bước lưới
                    startX: 20m,                      // lề trái
                    maxX: 800m                      // biên phải “ảo”
                );

                var updArgs = new
                {
                    roomId = Guid.Empty.ToString(),            // orchestrator sẽ thay bằng roomId thật
                    roomItemId = target.RoomItemId.ToString(),
                    x = newX,
                    y = newY
                };
                var argsElem = JsonDocument.Parse(JsonSerializer.Serialize(updArgs)).RootElement;

                return Task.FromResult(new LlmResponse
                {
                    Message = null,
                    ToolCalls = new List<LlmToolCall>
            {
                new LlmToolCall { Name = "update_item_position", Arguments = argsElem }
            }
                });
            }

            if (toolName == "update_item_position")
            {
                return Task.FromResult(new LlmResponse
                {
                    Message = "(mock) Đã sắp xếp lại vị trí đồ vật cho phù hợp.",
                    ToolCalls = new List<LlmToolCall>()
                });
            }

            return Task.FromResult(new LlmResponse
            {
                Message = "(mock) Không biết thao tác tiếp theo.",
                ToolCalls = new List<LlmToolCall>()
            });
        }

        private static (decimal x, decimal y) PickNonOverlappingPosition(
    decimal preferredX,
    decimal preferredY,
    List<RoomItemInRoomDto> existing,
    decimal step = 40m,
    decimal startX = 20m,
    decimal maxX = 800m)
        {
            // Toạ độ đã chiếm (so sánh theo grid điểm)
            var used = new HashSet<string>(
                existing.Select(i => $"{(i.X ?? 0m):0.####}|{(i.Y ?? 0m):0.####}"));

            decimal x = preferredX <= 0m ? startX : preferredX;
            decimal y = preferredY <= 0m ? 40m : preferredY;

            for (int guard = 0; guard < 5000; guard++)
            {
                var key = $"{x:0.####}|{y:0.####}";
                if (!used.Contains(key))
                    return (x, y);

                x += step;
                if (x > maxX)
                {
                    x = startX;
                    y += step;
                }
            }

            // fallback an toàn (gần như không bao giờ tới)
            return (preferredX, preferredY);
        }

    }
}

