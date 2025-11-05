using BusinessObject.DTO.Order;
using BusinessObject.DTO.Payment;
using BusinessObject.Enums;
using Microsoft.Extensions.Options;
using Net.payOS;
using Repo.IRepository;
using Repo.Repository;
using Service.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Service
{
    public class SubscriptionAuditService : ISubscriptionService
    {

        private readonly ISubscriptionRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly IUnitOfWork _uow;
        private readonly ISubscriptionRepository _subRepo;

        public SubscriptionAuditService(
            IUnitOfWork uow,
            ISubscriptionRepository subscription,
            IUserRepository user,
            ISubscriptionRepository subRepo
            )
            {
                _uow = uow;
                _repo = subscription;
                _userRepo = user;
                _subRepo = subRepo;
            }
        public async Task<CancelSubscriptionResult> CancelByUserAsync(Guid userId)
        {
            var sub = await _repo.GetCurrentByUserAsync(userId);
            var user = await _userRepo.GetUserByUserID(userId);
            if (sub == null)
                return new(false, "Không tìm thấy gói đăng ký hiện hành cho user.");

            if (sub.Status == SubscriptionStatus.Inactive || sub.Status == SubscriptionStatus.Cancelled)
                return new(true, "Gói đã ở trạng thái không hoạt động.");

            sub.Status = SubscriptionStatus.Cancelled;
            sub.CancelledAt = DateTime.UtcNow;
            sub.CancelAtPeriodEnd = false;
            sub.CurrentPeriodEnd = DateTime.UtcNow;
            _repo.UpdateAsync(sub);
            user.IsPremium = false;
            await _uow.SaveChangesAsync();
            return new(true, "Đã hủy gói đăng ký.");
        }
        public async Task<int> DeactivateInvalidSubscriptionsAsync()
        {
            var targets = await _subRepo.GetSubscriptionsNeedingDeactivateAsync();
            if (targets.Count == 0) return 0;

            var now = DateTime.UtcNow;
            foreach (var s in targets)
            {
                s.Status = SubscriptionStatus.Inactive; 
                s.CancelledAt ??= now;                  
            }

            await _subRepo.BulkUpdateAsync(targets);
            await _userRepo.SetPremiumOffAsync(targets.Select(x => x.UserId));

            return targets.Count;
        }
    }
}
