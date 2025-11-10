using BusinessObject.DTO.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.IService.IChat
{
    public interface IChatService
    {
        Task<QuickChatResponse> ChatAsync(QuickChatRequest req, Guid? userId, CancellationToken ct);

    }
}
