using BusinessObject.Models;
using DataAccess;
using Repo.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.Repository
{
    public class ChatHistoryRepository : IChatHistoryRepository
    {
        private readonly HomeTrackDBContext _db;
        public ChatHistoryRepository(HomeTrackDBContext db) => _db = db;

        public async Task AddAsync(ChatMessage msg, CancellationToken ct)
        {
            await _db.ChatMessages.AddAsync(msg, ct);
        }

        public Task SaveChangesAsync(CancellationToken ct)
        {
            return _db.SaveChangesAsync(ct);
        }
    }

}
