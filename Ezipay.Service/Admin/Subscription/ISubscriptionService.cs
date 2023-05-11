using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezipay.ViewModel.AdminViewModel;

namespace Ezipay.Service.Admin.Subscription
{
    public interface ISubscriptionService
    {
        Task<SubscriptionLogResponse> GetSubscriptionLogs(SearchRequest request);
    }
}
