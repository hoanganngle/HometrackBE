using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.IService;
using Service.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.ToolRunner
{
    public class MidnightRunner : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<MidnightRunner> _log;

        public MidnightRunner(IServiceProvider sp, ILogger<MidnightRunner> log)
        {
            _sp = sp;
            _log = log;
        }

        private static TimeZoneInfo GetVnTimeZone()
        {
            // Windows vs Linux/Docker có ID khác nhau
            if (OperatingSystem.IsWindows())
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tz = GetVnTimeZone();

            while (!stoppingToken.IsCancellationRequested)
            {
                var nowLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
                var nextMidnightLocal = nowLocal.Date.AddDays(1);      // 00:00 ngày kế tiếp
                var delay = nextMidnightLocal - nowLocal;
                if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

                _log.LogInformation("MidnightRunner sleeping until {Next} (VN time) — delay {Delay}",
                                    nextMidnightLocal, delay);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException) { break; }

                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    using var scope = _sp.CreateScope();
                    var svc = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
                    var affected = await svc.DeactivateInvalidSubscriptionsAsync();
                    _log.LogInformation("Midnight audit finished — affected: {Count}", affected);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Midnight audit error");
                }
            }
        }
    }
}
