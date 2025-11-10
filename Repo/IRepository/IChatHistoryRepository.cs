using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repo.IRepository
{
    public interface IChatHistoryRepository
    {
        Task AddAsync(ChatMessage msg, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
