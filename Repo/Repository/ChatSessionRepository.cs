using BusinessObject.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repo.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.Repository
{
    public class ChatSessionRepository : IChatSessionRepository
    {
        private readonly HomeTrackDBContext _db;
        public ChatSessionRepository(HomeTrackDBContext db) => _db = db;

        public Task<ChatSession> GetAsync(Guid id, CancellationToken ct)
        {
            return _db.ChatSessions
                .Include(x => x.Messages.OrderByDescending(m => m.CreatedAt))
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public Task<List<ChatSession>> ListByUserAsync(string userId, CancellationToken ct)
        {
            return _db.ChatSessions
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<ChatSession> CreateAsync(ChatSession session, CancellationToken ct)
        {
            await _db.ChatSessions.AddAsync(session, ct);
            return session;
        }

        public Task SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}
