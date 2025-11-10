using BusinessObject.DTO.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IService.IChat
{
    public interface IGeminiClient
    {
        Task<string> GenerateAsync(string? systemPrompt, string userText, CancellationToken ct);

        Task<LlmResponse> RunWithToolsAsync(
        string systemPrompt,
        string userMessage,
        IEnumerable<object> toolSchemas,
        CancellationToken ct = default);

        Task<LlmResponse> ContinueWithToolResultAsync(
            string systemPrompt,
            IEnumerable<object> toolSchemas,
            string toolName,
            object toolResult,
            CancellationToken ct = default);
    }
}
