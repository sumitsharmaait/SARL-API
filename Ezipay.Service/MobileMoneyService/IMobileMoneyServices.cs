using Ezipay.ViewModel.AirtimeViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezipay.ViewModel.AdminViewModel;
using Ezipay.Database;
using Ezipay.ViewModel.CardPaymentViewModel;

namespace Ezipay.Service.MobileMoneyService
{
    public interface IMobileMoneyServices
    {
        Task<AddMoneyAggregatorResponse> MobileMoneyService(PayMoneyAggregatoryRequest Request, long WalletUserId = 0);
        Task<AdminMobileMoneyLimitResponse> VerifyMobileMoneyLimit(AdminMobileMoneyLimitRequest request);
        Task<MobileMoneySenderDetail> VerifySenderIdNumberExistorNot(MobileMoneySenderDetailrequest request);
        Task<AddMoneyAggregatorResponse> PayBankTransferServiceForNGNbankflutter(ThirdpartyPaymentByCardRequest request, string headerToken);
        Task<AddMoneyAggregatorResponse>  GhanaMobileMobileService(PayMoneyAggregatoryRequest Request, long WalletUserId = 0);

    }
}
