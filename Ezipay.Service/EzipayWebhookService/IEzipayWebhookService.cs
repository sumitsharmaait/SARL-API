using Ezipay.ViewModel.AirtimeViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.EzipayWebhookService
{
    public interface IEzipayWebhookService
    {
        Task<AddMoneyAggregatorResponse> TvService(PayMoneyAggregatoryRequest Request, long WalletUserId = 0);
    }
}
