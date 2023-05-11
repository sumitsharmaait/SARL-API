using Ezipay.ViewModel.AirtimeViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.AirtimeService
{
    public interface IAirtimeService
    {
        Task<AddMoneyAggregatorResponse> AirtimeServices(PayMoneyAggregatoryRequest Request, long WalletUserId = 0);
    }
}
