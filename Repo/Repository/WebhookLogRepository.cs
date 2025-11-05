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
    // WebhookLogRepository.cs
    public class WebhookLogRepository : IWebhookLogRepository
    {
        private readonly HomeTrackDBContext _context;
        public WebhookLogRepository(HomeTrackDBContext db)
        {
            _context = db;
        }

        public async Task AddAsync(WebhookLog log)
        {
            await _context.WebhookLogs.AddAsync(log);
        }
    }

}
