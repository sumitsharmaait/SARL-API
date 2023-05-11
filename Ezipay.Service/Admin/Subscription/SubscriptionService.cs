using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezipay.Repository.AdminRepo.Subscription;
using Ezipay.ViewModel.AdminViewModel;

namespace Ezipay.Service.Admin.Subscription
{
    public class SubscriptionService : ISubscriptionService
    {
        private ISubscriptionRepository _subscriptionRepository;
        public SubscriptionService()
        {
            _subscriptionRepository = new SubscriptionRepository();
        }
        public async Task<SubscriptionLogResponse> GetSubscriptionLogs(SearchRequest request)
        {
            var result = new SubscriptionLogResponse();
            result.SubscriptionLogs = await _subscriptionRepository.GetSubscriptionLogs(request);
            if (result.SubscriptionLogs.Count > 0)
            {
                result.TotalCount = result.SubscriptionLogs[0].TotalCount;
            }
            return result;
        }
    }
}
