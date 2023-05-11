using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezipay.ViewModel.AdminViewModel;

namespace Ezipay.Repository.AdminRepo.Subscription
{
    public interface ISubscriptionRepository
    {
        Task<List<SubscriptionLogRecord>> GetSubscriptionLogs(SearchRequest request);
    }
}
