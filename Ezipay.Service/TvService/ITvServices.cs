using Ezipay.ViewModel.AirtimeViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.TvService
{
    public interface ITvServices
    {
        Task<AddMoneyAggregatorResponse> TvService(PayMoneyAggregatoryRequest Request, long WalletUserId = 0);
    }
}
