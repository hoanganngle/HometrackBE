using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.IRepository
{
    public interface IChatSessionRepository
    {
        Task<ChatSession> GetAsync(Guid id, CancellationToken ct);
        Task<List<ChatSession>> ListByUserAsync(string userId, CancellationToken ct);
        Task<ChatSession> CreateAsync(ChatSession session, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
