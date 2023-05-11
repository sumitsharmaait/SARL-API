using Ezipay.ViewModel.AirtimeViewModel;
using Ezipay.ViewModel.BillViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezipay.Service.BillPaymentService
{
    public interface IBillPaymentService
    {
        Task<AddMoneyAggregatorResponse> BillPaymentServicesAggregator(BillPayMoneyAggregatoryRequest Request, long WalletUserId = 0);

        Task<AddMoneyAggregatorResponse> GetBillPaymentServicesAggregator(BillPayMoneyAggregatoryRequest Request, long WalletUserId = 0);

       // Task<string> GetFee(PayMoneyAggregatoryRequest Request);
    }
}
