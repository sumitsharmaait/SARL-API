using Ezipay.ViewModel.AirtimeViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.InterNetProviderService
{
    public interface IInterNetProviderService
    {
        Task<AddMoneyAggregatorResponse> ISPServices(PayMoneyAggregatoryRequest Request, long WalletUserId = 0);
    }
}
